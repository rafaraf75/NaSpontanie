using NaSpontanie.API.Dtos.Users;
using NaSpontanie.API.Dtos.Comments;
using NaSpontanie.API.Dtos.Interests;

namespace NaSpontanie.API.Dtos.Events
{
    [Serializable]
    public class EventDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? InterestName { get; set; }
        public DateTime Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public UserDto? Creator { get; set; }
        public List<CommentDto>? Comments { get; set; }
        public List<EventInterestDto>? EventInterests { get; set; }
    }
}
