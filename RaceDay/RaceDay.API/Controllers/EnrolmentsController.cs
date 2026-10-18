using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Entities;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/enrolments/event/5 (Organiser only)
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.Status,
                    e.BibNumber,
                    e.EnrolmentDate,
                    Participant = new { e.Participant!.UserId, e.Participant.FirstName, e.Participant.LastName, e.Participant.Email },
                    Category = new { e.Category!.CategoryId, e.Category.CategoryName }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/my (Participant only)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == userId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.Status,
                    e.BibNumber,
                    e.EnrolmentDate,
                    Event = new { e.Event!.EventId, e.Event.EventName, e.Event.EventDate, e.Event.City },
                    Category = new { e.Category!.CategoryId, e.Category.CategoryName, e.Category.Distance },
                    Result = e.Result == null ? null : new { e.Result.FinishTime, e.Result.OverallPosition, e.Result.Status }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/enrolments (Participant only)
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol([FromBody] Enrolment dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (await _context.Enrolments.AnyAsync(e => e.ParticipantId == userId && e.EventId == dto.EventId))
                return BadRequest(new { message = "You have already enrolled in this event" });

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return NotFound(new { message = "Category not found" });
            if (category.EventId != dto.EventId) return BadRequest(new { message = "Category does not belong to this event" });

            var enrolment = new Enrolment
            {
                ParticipantId = userId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                Status = "Pending",
                EnrolmentDate = DateTime.Now
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), new { id = enrolment.EnrolmentId }, new { enrolment.EnrolmentId, enrolment.Status });
        }

        // PUT: api/enrolments/5/status (Organiser)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();

            enrolment.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Status updated" });
        }

        // DELETE: api/enrolments/5 (Participant own)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != userId) return Forbid();

            enrolment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
