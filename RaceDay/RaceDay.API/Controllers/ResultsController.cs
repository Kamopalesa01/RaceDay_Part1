using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Entities;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.EventId == eventId)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.OverallPosition,
                    r.CategoryPosition,
                    r.Pace,
                    r.Status,
                    Participant = new { r.Enrolment!.Participant!.FirstName, r.Enrolment.Participant.LastName },
                    Category = r.Enrolment.Category!.CategoryName
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CaptureResult([FromBody] Result dto)
        {
            var enrolment = await _context.Enrolments.FindAsync(dto.EnrolmentId);
            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
                return BadRequest(new { message = "Result already exists for this enrolment" });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition,
                Pace = dto.Pace,
                Status = dto.Status,
                RecordedDate = DateTime.Now
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResultsForEvent), new { eventId = enrolment.EventId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] Result dto)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();

            result.FinishTime = dto.FinishTime;
            result.OverallPosition = dto.OverallPosition;
            result.CategoryPosition = dto.CategoryPosition;
            result.Pace = dto.Pace;
            result.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(result);
        }
    }
}