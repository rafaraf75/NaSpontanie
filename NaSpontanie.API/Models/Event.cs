using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NaSpontanie.API.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; } = null!;

        public List<Comment> Comments { get; set; } = new();

        public List<EventInterest> EventInterests { get; set; } = new();
    }
}
