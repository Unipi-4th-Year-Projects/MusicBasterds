using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MusicBasterds.Models
{
    public class Album
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Who uploaded it
        public string UserName { get; set; } = string.Empty;

        // Basic album info
        [Required(ErrorMessage = "Album title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Artist is required.")]
        public string Artist { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        public string Genre { get; set; } = string.Empty;

        // Optional album image stored as bytes
        public byte[]? Image { get; set; }

        // Album links
        [MinLength(1, ErrorMessage = "You must provide at least one link.")]
        public List<LinkItem> Links { get; set; } = new();

        // Description with custom validation (50 words minimum)
        [Required(ErrorMessage = "Description is required.")]
        [CustomValidation(typeof(Album), nameof(ValidateDescription))]
        public string Description { get; set; } = string.Empty;

        // Ratings and comments
        public List<Rating> Ratings { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();

        // Timestamp
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Computed property - only counts albums that have been rated
        public double AverageRating => Ratings.Count > 0 ? Ratings.Average(r => r.Value) : -1;

        // Helper property for display
        public string DisplayRating => AverageRating >= 0 ? AverageRating.ToString("F1") : "Not rated";

        // Custom validation method
        public static ValidationResult? ValidateDescription(string description, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(description))
                return new ValidationResult("Description is required.");

            var wordCount = description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount < 50)
                return new ValidationResult("Description must be at least 50 words.");

            return ValidationResult.Success;
        }
    }

    // Supporting classes
    public class LinkItem
    {
        [Required(ErrorMessage = "Link is required.")]
        [Url(ErrorMessage = "Must be a valid URL.")]
        public string Url { get; set; } = string.Empty;
    }

    public class Rating
    {
        [Range(0, 100, ErrorMessage = "Rating must be between 0 and 100.")]
        public int Value { get; set; }

        public string UserName { get; set; } = string.Empty;
    }

    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Add this line
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; } = DateTime.Now;
        public string? OwnerResponse { get; set; } // Optional response by uploader
        public DateTime? ResponseDate { get; set; } // When owner responded
    }
}
