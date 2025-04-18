using Microsoft.AspNetCore.Mvc;
using NaSpontanie.API.Models;
using NaSpontanie.API.Services;
using NaSpontanie.API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace NaSpontanie.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_authService.GetUserByEmail(request.Email) != null)
                return BadRequest("Użytkownik o podanym e-mailu już istnieje.");

            _authService.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Zarejestrowano pomyślnie!");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _authService.GetUserByEmail(request.Email);
            if (user == null)
                return BadRequest("Nieprawidłowy e-mail lub hasło.");

            if (!_authService.VerifyPassword(request.Password, user.PasswordHash!, user.PasswordSalt!))
                return BadRequest("Nieprawidłowy e-mail lub hasło.");

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token });
        }
        [Authorize]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out var userId))
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();

            if (!_authService.VerifyPassword(request.OldPassword, user.PasswordHash!, user.PasswordSalt!))
                return BadRequest("Stare hasło jest nieprawidłowe.");

            _authService.CreatePasswordHash(request.NewPassword, out var newHash, out var newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;

            _context.SaveChanges();

            return Ok("Hasło zostało zmienione.");
        }
    }
    public class ChangePasswordRequest
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
