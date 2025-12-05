using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MusicBasterds.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public string? Bio { get; set; }
        public string? ProfileImagePath { get; set; }

        // Navigation properties
        public virtual ICollection<Album> UploadedAlbums { get; set; } = new List<Album>();
        public virtual ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
        public virtual ICollection<Comment> PostedComments { get; set; } = new List<Comment>();
    }
}