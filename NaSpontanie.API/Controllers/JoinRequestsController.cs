using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;

namespace NaSpontanie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JoinRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JoinRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/JoinRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JoinRequest>>> GetAll()
        {
            return await _context.JoinRequests
                .Include(j => j.User)
                .Include(j => j.Event)
                .ToListAsync();
        }

        // GET: api/JoinRequests/event/5
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<JoinRequest>>> GetByEvent(int eventId)
        {
            return await _context.JoinRequests
                .Where(j => j.EventId == eventId)
                .Include(j => j.User)
                .ToListAsync();
        }

        // POST: api/JoinRequests
        [HttpPost]
        public async Task<ActionResult<JoinRequest>> Create(JoinRequest request)
        {
            request.RequestedAt = DateTime.Now;
            request.IsAccepted = null;

            _context.JoinRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = request.Id }, request);
        }

        // PUT: api/JoinRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool isAccepted)
        {
            var request = await _context.JoinRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.IsAccepted = isAccepted;

            // Jeśli zaakceptowano – tworzymy Friendship
            if (isAccepted)
            {
                var friendship = new Friendship
                {
                    UserId = request.UserId,
                    FriendId = request.Event!.CreatorId 
                };

                _context.Friendships.Add(friendship);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        // GET: api/joinrequests/exists?userId=1&eventId=2
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> Exists(int userId, int eventId)
        {
            var exists = await _context.JoinRequests.AnyAsync(j =>
                j.UserId == userId && j.EventId == eventId);

            return Ok(exists);
        }
    }
}
