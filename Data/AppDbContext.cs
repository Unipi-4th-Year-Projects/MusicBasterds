using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicBasterds.Models;

namespace MusicBasterds.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Album> Albums { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tell EF Core that LinkItem is NOT an entity (just a complex type)
            builder.Entity<Album>()
                .OwnsMany(a => a.Links, l =>
                {
                    l.WithOwner().HasForeignKey("AlbumId");
                    l.Property<int>("Id"); // Add a shadow property for ordering
                    l.HasKey("Id");
                });


            // Comment - User relationship
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.PostedComments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment - Album relationship (FIXED: Explicitly configure one side only)
            builder.Entity<Comment>()
                .HasOne(c => c.Album)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            // Rating - User relationship
            builder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.GivenRatings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rating - Album relationship
            builder.Entity<Rating>()
                .HasOne(r => r.Album)
                .WithMany(a => a.Ratings)
                .HasForeignKey(r => r.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment - Parent Comment relationship (for nested replies)
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete replies

            // Unique constraint: One rating per user per album
            builder.Entity<Rating>()
                .HasIndex(r => new { r.AlbumId, r.UserId })
                .IsUnique();
        }
    }
}