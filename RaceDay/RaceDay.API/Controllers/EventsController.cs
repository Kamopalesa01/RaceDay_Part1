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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events (Public)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? city, [FromQuery] DateTime? date)
        {
            var query = _context.Events.Where(e => e.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(city))
                query = query.Where(e => e.City.Contains(city));

            if (date.HasValue)
                query = query.Where(e => e.EventDate.Date == date.Value.Date);

            var events = await query
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.EventDescription,
                    e.EventDate,
                    e.StartTime,
                    e.EndTime,
                    e.Venue,
                    e.City,
                    e.Province,
                    e.MaxParticipants,
                    OrganiserName = e.Organiser != null ? e.Organiser.FirstName + " " + e.Organiser.LastName : "Unknown",
                    CategoryCount = e.Categories.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5 (Public)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            return Ok(new
            {
                ev.EventId,
                ev.EventName,
                ev.EventDescription,
                ev.EventDate,
                ev.StartTime,
                ev.EndTime,
                ev.Venue,
                ev.City,
                ev.Province,
                ev.MaxParticipants,
                ev.IsActive,
                Organiser = ev.Organiser == null ? null : new
                {
                    ev.Organiser.UserId,
                    ev.Organiser.FirstName,
                    ev.Organiser.LastName,
                    ev.Organiser.Email
                },
                Categories = ev.Categories.Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.Distance,
                    c.EntryFee,
                    c.MaxParticipants
                })
            });
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] Event dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var newEvent = new Event
            {
                OrganiserId = userId,
                EventName = dto.EventName,
                EventDescription = dto.EventDescription,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Venue = dto.Venue,
                City = dto.City,
                Province = dto.Province,
                MaxParticipants = dto.MaxParticipants,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, new { newEvent.EventId, newEvent.EventName });
        }

        // PUT: api/events/5 (Organiser only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != userId)
                return Forbid();

            ev.EventName = dto.EventName;
            ev.EventDescription = dto.EventDescription;
            ev.EventDate = dto.EventDate;
            ev.StartTime = dto.StartTime;
            ev.EndTime = dto.EndTime;
            ev.Venue = dto.Venue;
            ev.City = dto.City;
            ev.Province = dto.Province;
            ev.MaxParticipants = dto.MaxParticipants;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully" });
        }

        // DELETE: api/events/5 (Organiser only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != userId)
                return Forbid();

            ev.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}