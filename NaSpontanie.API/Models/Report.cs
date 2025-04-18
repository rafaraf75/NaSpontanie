using System.ComponentModel.DataAnnotations;

namespace NaSpontanie.API.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Required]
        public int ReporterId { get; set; }
        public required User Reporter { get; set; }

        public int? ReportedUserId { get; set; }
        public User? ReportedUser { get; set; }

        public int? ReportedEventId { get; set; }
        public Event? ReportedEvent { get; set; }

        [Required]
        public required string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
