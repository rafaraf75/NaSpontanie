using System.ComponentModel.DataAnnotations;

namespace NaSpontanie.API.Dtos.Events
{
    public class CreateEventDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public List<int> InterestIds { get; set; } = new List<int>();
    }
}
