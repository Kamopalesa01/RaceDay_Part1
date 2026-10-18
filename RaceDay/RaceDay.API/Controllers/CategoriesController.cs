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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/categories/event/5 (Public)
        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/categories (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory([FromBody] Category dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            if (ev.OrganiserId != userId) return Forbid();

            var category = new Category
            {
                EventId = dto.EventId,
                CategoryName = dto.CategoryName,
                Distance = dto.Distance,
                EntryFee = dto.EntryFee,
                MaxParticipants = dto.MaxParticipants
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoriesForEvent), new { eventId = category.EventId }, category);
        }

        // PUT: api/categories/5 (Organiser only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound(new { message = "Category not found" });
            if (category.Event!.OrganiserId != userId) return Forbid();

            category.CategoryName = dto.CategoryName;
            category.Distance = dto.Distance;
            category.EntryFee = dto.EntryFee;
            category.MaxParticipants = dto.MaxParticipants;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE: api/categories/5 (Organiser only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != userId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}