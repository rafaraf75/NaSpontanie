namespace NaSpontanie.API.Models
{
    // Tabela pośrednia wiele-do-wielu między Event a Interest
    public class EventInterest
    {
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int InterestId { get; set; }
        public Interest Interest { get; set; } = null!;
    }
}
