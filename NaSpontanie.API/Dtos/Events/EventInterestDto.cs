using NaSpontanie.API.Dtos.Interests;

namespace NaSpontanie.API.Dtos.Events
{
    public class EventInterestDto
    {
        public int InterestId { get; set; }
        public InterestDto? Interest { get; set; }
    }
}
