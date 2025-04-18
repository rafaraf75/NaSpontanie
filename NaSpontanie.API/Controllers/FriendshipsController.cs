using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;

namespace NaSpontanie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendshipsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FriendshipsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Friendships/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetFriends(int userId)
        {
            var friends = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.Friend)
                .ToListAsync();

            return Ok(friends);
        }

        // POST: api/Friendships
        [HttpPost]
        public async Task<ActionResult<Friendship>> AddFriendship(Friendship friendship)
        {
            // Sprawdź, czy już istnieje
            var exists = await _context.Friendships.AnyAsync(f =>
                f.UserId == friendship.UserId && f.FriendId == friendship.FriendId);

            if (exists)
                return BadRequest("Taka znajomość już istnieje.");

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFriends), new { userId = friendship.UserId }, friendship);
        }

        // DELETE: api/Friendships?userId=1&friendId=2
        [HttpDelete]
        public async Task<IActionResult> DeleteFriendship(int userId, int friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId);

            if (friendship == null)
                return NotFound();

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
