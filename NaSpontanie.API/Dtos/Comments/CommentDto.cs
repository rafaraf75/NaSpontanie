using NaSpontanie.API.Dtos.Users;

namespace NaSpontanie.API.Dtos.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int UserId { get; set; }
        public UserDto? User { get; set; }
        public int EventId { get; set; }
    }
}
