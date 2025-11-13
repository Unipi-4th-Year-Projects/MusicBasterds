namespace MusicBasterds.Models
{
    public class UserStats
    {
        public string Username { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int AlbumCount { get; set; }
    }
}