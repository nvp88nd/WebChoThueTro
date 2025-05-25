using Microsoft.EntityFrameworkCore;
using WebNhaTro.Models;

namespace WebNhaTro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính cho bảng Favorites (UserID, PostID)
            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserID, f.PostID });

            // Cấu hình mối quan hệ giữa Users và Posts
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.OwnerID);

            // Cấu hình mối quan hệ giữa Posts và PostImages
            modelBuilder.Entity<PostImage>()
                .HasOne(pi => pi.Post)
                .WithMany(p => p.PostImages)
                .HasForeignKey(pi => pi.PostID);

            // Cấu hình mối quan hệ giữa Users và Favorites
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserID);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Post)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PostID);
        }
    }
}
