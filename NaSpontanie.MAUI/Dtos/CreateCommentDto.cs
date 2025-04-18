namespace NaSpontanie.MAUI.Dtos
{
    public class CreateCommentDto
    {
        public string Text { get; set; } = string.Empty;
        public int EventId { get; set; }
        public int UserId { get; set; }
    }
}
