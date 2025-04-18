using System.ComponentModel.DataAnnotations;

namespace NaSpontanie.API.Models
{
    public class JoinRequest
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event? Event { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public bool? IsAccepted { get; set; }
    }
}
