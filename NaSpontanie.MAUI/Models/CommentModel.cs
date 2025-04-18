namespace NaSpontanie.MAUI.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
