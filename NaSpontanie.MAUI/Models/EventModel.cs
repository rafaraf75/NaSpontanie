namespace NaSpontanie.MAUI.Models
{
    public class EventModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string InterestName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public UserModel? Creator { get; set; }
        public List<CommentModel>? Comments { get; set; }
        public List<EventInterestModel>? EventInterests { get; set; }
    }
}
