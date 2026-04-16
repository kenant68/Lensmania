using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Database;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }

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
      			.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(p => p.UserId);
        });
    }
}
