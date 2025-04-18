namespace NaSpontanie.MAUI.Dtos;

public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InterestName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceKm { get; set; }

    public UserDto? Creator { get; set; }
    public List<CommentDto>? Comments { get; set; }
    public List<EventInterestDto>? EventInterests { get; set; }
}
