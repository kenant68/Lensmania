using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Database;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
			entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
			entity.Property(u => u.Email).IsRequired();

			entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });
        
        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(p => p.Title).HasMaxLength(150);
            entity.Property(p => p.PhotoUrl).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(300);

			entity.HasOne(p => p.User)
      			.WithMany(u => u.Posts)
      			.HasForeignKey(p => p.UserId)
      			.OnDelete(DeleteBehavior.Restrict);

			entity.HasIndex(p => p.UserId);
        });
        
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
	        entity.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
	        entity.Property(t => t.RowVersion).IsRowVersion();

	        entity.HasIndex(t => t.TokenHash).IsUnique();
	        entity.HasIndex(t => new { t.UserId, t.ConsumedAt });

	        entity.HasOne(t => t.User)
		        .WithMany()
		        .HasForeignKey(t => t.UserId)
		        .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(l => new { l.UserId, l.PostId });

            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
