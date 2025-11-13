using System.Text.Json;
using Microsoft.JSInterop;
using MusicBasterds.Models;

namespace MusicBasterds.Services
{
    public class AlbumService
    {
        private readonly IJSRuntime _js;
        private const string StorageKey = "albums";

        public AlbumService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<List<Album>> GetAllAlbumsAsync()
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json)) return new List<Album>();

            try
            {
                return JsonSerializer.Deserialize<List<Album>>(json) ?? new List<Album>();
            }
            catch
            {
                return new List<Album>();
            }
        }

        private async Task SaveAlbumsAsync(List<Album> albums)
        {
            var json = JsonSerializer.Serialize(albums);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }

        public async Task AddAlbumAsync(Album album)
        {
            var albums = await GetAllAlbumsAsync();
            albums.Add(album);
            await SaveAlbumsAsync(albums);
        }

        public async Task<List<Album>> GetUserAlbumsAsync(string username)
        {
            var albums = await GetAllAlbumsAsync();
            return albums.Where(a => a.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<Album?> GetAlbumByIdAsync(Guid id)
        {
            var albums = await GetAllAlbumsAsync();
            return albums.FirstOrDefault(a => a.Id == id);
        }

        public async Task AddRatingAsync(Guid albumId, Rating rating)
        {
            var albums = await GetAllAlbumsAsync();
            var album = albums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                album.Ratings.Add(rating);
                await SaveAlbumsAsync(albums);
            }
        }

        public async Task AddCommentAsync(Guid albumId, Comment comment)
        {
            var albums = await GetAllAlbumsAsync();
            var album = albums.FirstOrDefault(a => a.Id == albumId);
            if (album != null)
            {
                album.Comments.Add(comment);
                await SaveAlbumsAsync(albums);
            }
        }

        // --- Statistics ---
        public async Task<List<Album>> GetTopAlbumsAllTimeAsync(int count = 5)
        {
            var albums = await GetAllAlbumsAsync();
            return albums
                .Where(a => a.Ratings.Count > 0) // Only include rated albums
                .OrderByDescending(a => a.AverageRating)
                .Take(count)
                .ToList();
        }

        public async Task<List<Album>> GetTopAlbumsThisWeekAsync(int count = 5)
        {
            var albums = await GetAllAlbumsAsync();
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            return albums
                .Where(a => a.UploadDate >= oneWeekAgo && a.Ratings.Count > 0) // Only include rated albums
                .OrderByDescending(a => a.AverageRating)
                .Take(count)
                .ToList();
        }

        public async Task<List<Album>> GetLatestAlbumsAsync(int count = 5)
        {
            var albums = await GetAllAlbumsAsync();
            return albums.OrderByDescending(a => a.UploadDate).Take(count).ToList();
        }


        public async Task UpdateCommentAsync(Guid albumId, Comment updatedComment)
        {
            var albums = await GetAllAlbumsAsync();
            var album = albums.FirstOrDefault(a => a.Id == albumId);

            if (album != null)
            {
                // Find and update the comment based on its Guid
                var commentIndex = album.Comments.FindIndex(c => c.Id == updatedComment.Id);
                if (commentIndex >= 0)
                {
                    // Replace the entire comment object safely
                    album.Comments[commentIndex] = updatedComment;
                }
                else
                {
                    // If not found, add it (safety net)
                    album.Comments.Add(updatedComment);
                }

                await SaveAlbumsAsync(albums);
            }
        }


        public async Task<List<UserStats>> GetTopUsersThisWeekAsync(int count = 5)
        {
            var albums = await GetAllAlbumsAsync();
            var oneWeekAgo = DateTime.Now.AddDays(-7);

            var recentAlbums = albums.Where(a => a.UploadDate >= oneWeekAgo).ToList();

            var userStats = recentAlbums
                .GroupBy(a => a.UserName)
                .Select(g => new UserStats
                {
                    Username = g.Key,
                    AverageRating = g.Average(a => a.AverageRating),
                    AlbumCount = g.Count()
                })
                .Where(u => u.AlbumCount >= 1) // At least one album this week
                .OrderByDescending(u => u.AverageRating)
                .Take(count)
                .ToList();

            return userStats;
        }

        public async Task<double> GetUserAverageRatingAsync(string username)
        {
            var userAlbums = await GetUserAlbumsAsync(username);
            var ratedAlbums = userAlbums.Where(a => a.Ratings.Count > 0).ToList();

            return ratedAlbums.Any() ? ratedAlbums.Average(a => a.AverageRating) : 0;
        }
    }
}
