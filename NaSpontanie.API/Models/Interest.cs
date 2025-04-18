using System.ComponentModel.DataAnnotations;

namespace NaSpontanie.API.Models
{
    public class Interest
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public ICollection<EventInterest> EventInterests { get; set; } = new List<EventInterest>();
    }
}
