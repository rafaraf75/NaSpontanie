using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;
using NaSpontanie.API.Dtos.Events;
using NaSpontanie.API.Dtos.Users;
using NaSpontanie.API.Dtos.Comments;
using NaSpontanie.API.Dtos.Interests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NaSpontanie.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
        {
            var events = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Comments).ThenInclude(c => c.User)
                .Include(e => e.EventInterests).ThenInclude(ei => ei.Interest)
                .ToListAsync();

            var eventDtos = events.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Creator = e.Creator == null ? null : new UserDto
                {
                    Id = e.Creator.Id,
                    Username = e.Creator.Username,
                    Email = e.Creator.Email,
                    Bio = e.Creator.Bio,
                    Latitude = e.Creator.Latitude,
                    Longitude = e.Creator.Longitude
                },
                Comments = e.Comments?.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    UserId = c.UserId,
                    User = c.User == null ? null : new UserDto
                    {
                        Id = c.User.Id,
                        Username = c.User.Username,
                        Email = c.User.Email,
                        Bio = c.User.Bio,
                        Latitude = c.User.Latitude,
                        Longitude = c.User.Longitude
                    }
                }).ToList(),
                EventInterests = e.EventInterests?.Select(ei => new EventInterestDto
                {
                    InterestId = ei.InterestId,
                    Interest = ei.Interest == null ? null : new InterestDto
                    {
                        Id = ei.Interest.Id,
                        Name = ei.Interest.Name
                    }
                }).ToList(),
                InterestName = e.EventInterests?.FirstOrDefault()?.Interest?.Name
            }).ToList();

            return Ok(eventDtos);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetById(int id)
        {
            var e = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Comments).ThenInclude(c => c.User)
                .Include(e => e.EventInterests).ThenInclude(ei => ei.Interest)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (e == null)
                return NotFound();

            var dto = new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Creator = e.Creator == null ? null : new UserDto
                {
                    Id = e.Creator.Id,
                    Username = e.Creator.Username,
                    Email = e.Creator.Email,
                    Bio = e.Creator.Bio,
                    Latitude = e.Creator.Latitude,
                    Longitude = e.Creator.Longitude
                },
                Comments = e.Comments?.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    UserId = c.UserId,
                    User = c.User == null ? null : new UserDto
                    {
                        Id = c.User.Id,
                        Username = c.User.Username,
                        Email = c.User.Email,
                        Bio = c.User.Bio,
                        Latitude = c.User.Latitude,
                        Longitude = c.User.Longitude
                    }
                }).ToList(),
                EventInterests = e.EventInterests?.Select(ei => new EventInterestDto
                {
                    InterestId = ei.InterestId,
                    Interest = ei.Interest == null ? null : new InterestDto
                    {
                        Id = ei.Interest.Id,
                        Name = ei.Interest.Name
                    }
                }).ToList(),
                InterestName = e.EventInterests?.FirstOrDefault()?.Interest?.Name
            };

            return Ok(dto);
        }

        // POST: api/Events
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EventDto>> Create(CreateEventDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int creatorId))
                return BadRequest("Nie można ustalić ID użytkownika z tokena (brak claim).");

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CreatorId = creatorId
            };

            newEvent.EventInterests = dto.InterestIds.Select(interestId => new EventInterest
            {
                InterestId = interestId,
                Event = newEvent
            }).ToList();

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, null);
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Event ev)
        {
            if (id != ev.Id)
                return BadRequest();

            _context.Entry(ev).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
