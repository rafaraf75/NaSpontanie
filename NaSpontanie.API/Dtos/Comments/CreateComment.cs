namespace NaSpontanie.API.Dtos.Comments
{
    public class CreateCommentDto
    {
        public string Text { get; set; } = string.Empty;
        public int EventId { get; set; }
    }
}
