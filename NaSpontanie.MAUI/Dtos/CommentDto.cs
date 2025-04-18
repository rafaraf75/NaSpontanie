namespace NaSpontanie.MAUI.Dtos;

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int UserId { get; set; }
    public UserDto? User { get; set; }
    public int EventId { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReport { get; set; }

    public string UserIdAsString { get; set; } = "";
}
