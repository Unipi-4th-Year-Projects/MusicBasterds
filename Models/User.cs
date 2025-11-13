namespace MusicBasterds.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // In production, hash this!
        public string Email { get; set; } = string.Empty;
    }
}
