using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NaSpontanie.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public byte[]? PasswordHash { get; set; }

        [Required]
        public byte[]? PasswordSalt { get; set; }

        public string? Bio { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public ICollection<Comment>? Comments { get; set; }

        public ICollection<Friendship>? Friends { get; set; }

        public ICollection<Event>? CreatedEvents { get; set; }
    }
}
