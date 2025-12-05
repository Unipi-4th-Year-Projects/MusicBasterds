public class UserStats
{
    public string Username { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int AlbumCount { get; set; }
    public int TotalRatingsGiven { get; set; }
    public int TotalCommentsPosted { get; set; }
    public double AverageAlbumRating { get; set; } // Average of all their albums
    public DateTime? LastActivity { get; set; }
    public Dictionary<string, int> GenreDistribution { get; set; } = new();
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // How they rate others
}