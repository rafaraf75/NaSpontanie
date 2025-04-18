using System.ComponentModel.DataAnnotations;

namespace NaSpontanie.API.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public required string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? User { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}
