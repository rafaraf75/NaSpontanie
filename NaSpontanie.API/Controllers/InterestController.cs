using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;

namespace NaSpontanie.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InterestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Interests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Interest>>> GetAll()
        {
            return await _context.Interests.ToListAsync();
        }

        // GET: api/Interests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Interest>> GetById(int id)
        {
            var interest = await _context.Interests.FindAsync(id);

            if (interest == null)
                return NotFound();

            return interest;
        }

        // POST: api/Interests
        [HttpPost]
        public async Task<ActionResult<Interest>> Create(Interest interest)
        {
            _context.Interests.Add(interest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = interest.Id }, interest);
        }

        // PUT: api/Interests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Interest updatedInterest)
        {
            if (id != updatedInterest.Id)
                return BadRequest();

            _context.Entry(updatedInterest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Interests.Any(i => i.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Interests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var interest = await _context.Interests.FindAsync(id);
            if (interest == null)
                return NotFound();

            _context.Interests.Remove(interest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
