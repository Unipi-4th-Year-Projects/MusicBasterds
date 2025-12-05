using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicBasterds.Models
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AlbumId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Comment content is required.")]
        [MinLength(3, ErrorMessage = "Comment must be at least 3 characters.")]
        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string Content { get; set; } = string.Empty;

        public DateTime PostedAt { get; set; } = DateTime.Now;

        // Optional response by album uploader
        public string? OwnerResponse { get; set; }
        public DateTime? ResponseDate { get; set; }

        // For nested replies
        public Guid? ParentCommentId { get; set; }

        // Navigation properties
        public virtual Album? Album { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // UI state (not stored in DB)
        [System.Text.Json.Serialization.JsonIgnore]
        public bool ShowReplies { get; set; } = false;
    }
}