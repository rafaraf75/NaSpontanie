namespace NaSpontanie.API.Dtos.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
