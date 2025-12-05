using System;
using System.ComponentModel.DataAnnotations;

namespace MusicBasterds.Models
{
    public class Rating
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AlbumId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Rating must be between 0 and 100.")]
        public int Value { get; set; }

        public DateTime RatedAt { get; set; } = DateTime.Now;

        // Optional text review
        [MaxLength(500)]
        public string? Review { get; set; }

        // Navigation properties
        public virtual Album? Album { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}