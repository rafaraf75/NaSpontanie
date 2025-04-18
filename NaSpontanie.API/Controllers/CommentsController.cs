using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;
using NaSpontanie.API.Dtos.Comments;

namespace NaSpontanie.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Comments?eventId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetByEventId([FromQuery] int? eventId)
        {
            var query = _context.Comments
                .Include(c => c.User)
                .Include(c => c.Event)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(c => c.EventId == eventId);

            var comments = await query.ToListAsync();

            var result = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                UserId = c.UserId,

                User = c.User == null
                    ? null
                    : new Dtos.Users.UserDto
                    {
                        Id = c.User.Id,
                        Username = c.User.Username,
                        Email = c.User.Email,
                        Bio = c.User.Bio,
                        Latitude = c.User.Latitude,
                        Longitude = c.User.Longitude
                    },
                EventId = c.EventId
            });

            return Ok(result);
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetById(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            var result = new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                UserId = comment.UserId,
                EventId = comment.EventId,
                User = comment.User == null
                    ? null
                    : new Dtos.Users.UserDto
                    {
                        Id = comment.User.Id,
                        Username = comment.User.Username,
                        Email = comment.User.Email,
                        Bio = comment.User.Bio,
                        Latitude = comment.User.Latitude,
                        Longitude = comment.User.Longitude
                    }
            };

            return Ok(result);
        }

        // POST: api/Comments
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> Create([FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest(new { error = "Text is required" });

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
                return Unauthorized("Nie można ustalić ID użytkownika z tokena.");

            int userId = int.Parse(userIdStr);

            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null)
                return BadRequest("Nie znaleziono wydarzenia");

            var comment = new Comment
            {
                Text = dto.Text,
                UserId = userId,
                EventId = dto.EventId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdCommentDto = new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                UserId = userId,
                EventId = comment.EventId
            };

            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, createdCommentDto);
        }

        // PUT: api/Comments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommentDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            comment.Text = dto.Text;
            comment.EventId = dto.EventId;
            comment.UserId = dto.UserId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
