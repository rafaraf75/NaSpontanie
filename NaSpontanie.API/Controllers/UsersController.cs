using Microsoft.AspNetCore.Mvc;
using NaSpontanie.API.Models;
using NaSpontanie.API.Data;
using NaSpontanie.API.Dtos;
using NaSpontanie.API.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using NaSpontanie.API.Services;
using System.Security.Claims;

namespace NaSpontanie.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                Latitude = user.Latitude,
                Longitude = user.Longitude
            };

            return Ok(userDto);
        }

        // POST: api/users
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        // ZMIENIONY: PUT: api/users/5
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            // Aktualizacja tylko przekazanych pól
            user.Username = dto.Username ?? user.Username;
            user.Email = dto.Email ?? user.Email;
            user.Bio = dto.Bio ?? user.Bio;

            if (dto.Latitude.HasValue)
                user.Latitude = dto.Latitude.Value;

            if (dto.Longitude.HasValue)
                user.Longitude = dto.Longitude.Value;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        // PUT: api/users/{id}/location
        [HttpPut("{id}/location")]
        public IActionResult UpdateLocation(int id, [FromBody] LocationDto location)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            user.Latitude = location.Latitude;
            user.Longitude = location.Longitude;

            _context.SaveChanges();

            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out var userId))
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                Latitude = user.Latitude,
                Longitude = user.Longitude
            };

            return Ok(userDto);
        }
        [Authorize]
        [HttpPut("{id}/password")]
        public IActionResult ChangePassword(int id, [FromBody] PasswordUpdateDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub != id.ToString())
                return Forbid("Nie możesz zmieniać hasła innego użytkownika.");

            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();

            // Weryfikacja starego hasła
            if (!authService.VerifyPassword(dto.CurrentPassword, user.PasswordHash!, user.PasswordSalt!))
                return BadRequest("Nieprawidłowe obecne hasło.");

            // Nowe hasło
            authService.CreatePasswordHash(dto.NewPassword, out byte[] newHash, out byte[] newSalt);

            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;

            _context.SaveChanges();

            return NoContent();
        }
    }
}
