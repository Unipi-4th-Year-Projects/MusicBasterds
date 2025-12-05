using Microsoft.EntityFrameworkCore;
using MusicBasterds.Data;
using MusicBasterds.Models;
using System.Security.Claims;

namespace MusicBasterds.Services
{
    public class AlbumService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AlbumService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Add these methods to AlbumService.cs

        public async Task<List<Album>> GetAllAlbumsAsync()
        {
            return await _context.Albums
                .Include(a => a.Ratings)
                .Include(a => a.Comments)
                .ThenInclude(c => c.Replies)
                .Include(a => a.User)
                .OrderByDescending(a => a.UploadDate)
                .ToListAsync();
        }

        public async Task<Album?> GetAlbumAsync(Guid id)
        {
            return await _context.Albums
                .Include(a => a.Ratings)
                .Include(a => a.Comments)
                .ThenInclude(c => c.Replies)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Album>> GetAlbumsForUserAsync(string userId)
        {
            return await _context.Albums
                .Where(a => a.UserId == userId)
                .Include(a => a.Ratings)
                .Include(a => a.Comments)
                .ThenInclude(c => c.Replies)
                .OrderByDescending(a => a.UploadDate)
                .ToListAsync();
        }

        public async Task<double> GetUserAverageRatingAsync(string userId)
        {
            var userAlbums = await _context.Albums
                .Where(a => a.UserId == userId)
                .Include(a => a.Ratings)
                .ToListAsync();

            if (!userAlbums.Any()) return 0;

            var albumsWithRatings = userAlbums.Where(a => a.Ratings.Count > 0).ToList();
            if (!albumsWithRatings.Any()) return 0;

            return albumsWithRatings.Average(a => a.AverageRating);
        }

        public async Task<Album> CreateAlbumAsync(Album album, string userId, string userName)
        {
            album.Id = Guid.NewGuid();
            album.UserId = userId;
            album.UserName = userName;
            album.UploadDate = DateTime.Now;

            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
            return album;
        }

        public async Task<bool> AddOrUpdateRatingAsync(Guid albumId, string userId, string userName, int ratingValue)
        {
            var album = await _context.Albums
                .Include(a => a.Ratings)
                .FirstOrDefaultAsync(a => a.Id == albumId);

            if (album == null) return false;

            var existingRating = album.Ratings.FirstOrDefault(r => r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Value = ratingValue;
                existingRating.RatedAt = DateTime.Now;
            }
            else
            {
                album.Ratings.Add(new Rating
                {
                    Id = Guid.NewGuid(),
                    AlbumId = albumId,
                    UserId = userId,
                    UserName = userName,
                    Value = ratingValue,
                    RatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Comment> AddCommentAsync(Guid albumId, string userId, string userName, string content)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                AlbumId = albumId,
                UserId = userId,
                UserName = userName,
                Content = content,
                PostedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> AddReplyAsync(Guid albumId, Guid parentCommentId, string userId, string userName, string content)
        {
            var parentComment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == parentCommentId && c.AlbumId == albumId);

            if (parentComment == null) return null;

            var reply = new Comment
            {
                Id = Guid.NewGuid(),
                AlbumId = albumId,
                UserId = userId,
                UserName = userName,
                Content = content,
                PostedAt = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<bool> AddOwnerResponseAsync(Guid commentId, string response, string ownerUserId)
        {
            var comment = await _context.Comments
                .Include(c => c.Album)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null || comment.Album?.UserId != ownerUserId)
                return false;

            comment.OwnerResponse = response;
            comment.ResponseDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAlbumAsync(Guid albumId)
        {
            var album = await _context.Albums.FindAsync(albumId);
            if (album == null) return false;

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserStats>> GetTopUsersAsync(int count = 10)
        {
            return await _context.Users
                .Select(u => new UserStats
                {
                    UserId = u.Id,
                    Username = u.UserName!,
                    AlbumCount = u.UploadedAlbums.Count,
                    AverageRating = u.UploadedAlbums
                        .Where(a => a.Ratings.Count > 0)
                        .Select(a => a.AverageRating)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .OrderByDescending(u => u.AlbumCount)
                .ThenByDescending(u => u.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        // Image handling
        public async Task<string> SaveAlbumImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        // Add to AlbumService.cs

        public async Task<bool> UpdateAlbumAsync(Album album)
        {
            var existingAlbum = await _context.Albums
                .Include(a => a.Links)
                .FirstOrDefaultAsync(a => a.Id == album.Id);

            if (existingAlbum == null) return false;

            // Update properties
            existingAlbum.Title = album.Title;
            existingAlbum.Artist = album.Artist;
            existingAlbum.Genre = album.Genre;
            existingAlbum.Year = album.Year;
            existingAlbum.Description = album.Description;

            // Update image if provided
            if (album.Image != null && album.Image.Length > 0)
            {
                existingAlbum.Image = album.Image;
                existingAlbum.ImageContentType = album.ImageContentType;
            }

            // Clear existing links and add new ones
            _context.RemoveRange(existingAlbum.Links);
            existingAlbum.Links = album.Links;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}