using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MusicBasterds.Models
{
    public class Album
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Identity integration
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // Basic album info
        [Required(ErrorMessage = "Album title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Artist is required.")]
        [MaxLength(200)]
        public string Artist { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        public string Genre { get; set; } = string.Empty;

        // Optional: Add Year field
        [Range(1900, 2100)]
        public int? Year { get; set; }

        // Optional album image stored as bytes
        public byte[]? Image { get; set; }

        // Optional: Store image content type
        public string? ImageContentType { get; set; }

        // Album links
        [MinLength(1, ErrorMessage = "You must provide at least one link.")]
        public List<LinkItem> Links { get; set; } = new();

        // Description - REMOVE the CustomValidation attribute
        [Required(ErrorMessage = "Description is required.")]
        [MinLength(50, ErrorMessage = "Description must be at least 50 characters.")]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Ratings and comments (navigation properties)
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Timestamp
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Navigation property for Identity
        public virtual ApplicationUser? User { get; set; }

        // Computed property - only counts albums that have been rated
        public double AverageRating => Ratings != null && Ratings.Any() ? Ratings.Average(r => r.Value) : -1;

        // Helper property for display
        public string DisplayRating => AverageRating >= 0 ? AverageRating.ToString("F1") : "Not rated";
    }

    public class LinkItem
    {
        [Required(ErrorMessage = "Link is required.")]
        [Url(ErrorMessage = "Must be a valid URL.")]
        public string Url { get; set; } = string.Empty;
    }
}