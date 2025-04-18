using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;

namespace NaSpontanie.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventInterestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventInterestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EventInterests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventInterest>>> GetAll()
        {
            return await _context.EventInterests
                .Include(ei => ei.Event)
                .Include(ei => ei.Interest)
                .ToListAsync();
        }

        // GET: api/EventInterests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventInterest>> GetById(int id)
        {
            var eventInterest = await _context.EventInterests
                .Include(ei => ei.Event)
                .Include(ei => ei.Interest)
                .FirstOrDefaultAsync(ei => ei.EventId == id);

            if (eventInterest == null)
                return NotFound();

            return eventInterest;
        }

        // POST: api/EventInterests
        [HttpPost]
        public async Task<ActionResult<EventInterest>> Create(EventInterest eventInterest)
        {
            _context.EventInterests.Add(eventInterest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = eventInterest.EventId }, eventInterest);
        }

        // DELETE: api/EventInterests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eventInterest = await _context.EventInterests.FindAsync(id);
            if (eventInterest == null)
                return NotFound();

            _context.EventInterests.Remove(eventInterest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
