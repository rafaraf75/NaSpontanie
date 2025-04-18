namespace NaSpontanie.MAUI.Models
{
    public class JoinRequestModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime RequestedAt { get; set; }
        public bool? IsAccepted { get; set; }
    }
}
