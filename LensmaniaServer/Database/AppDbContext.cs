using LensmaniaServer.Models;
using LensmaniaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Database;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Theme> Themes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Theme>(entity =>
		{
        	entity.HasKey(t => t.Id);

        	entity.Property(t => t.Name)
            	.IsRequired()
            	.HasMaxLength(100);

        	entity.Property(t => t.Icon)
            	.HasMaxLength(400);
		});

        modelBuilder.Entity<Event>(entity =>
        {
        	entity.HasKey(e => e.Id);

        	entity.Property(e => e.Name)
            	.IsRequired()
            	.HasMaxLength(200);

        	entity.Property(e => e.StartDate).IsRequired();
        	entity.Property(e => e.EndDate).IsRequired();
        	entity.Property(e => e.IsPremium).IsRequired();

        	entity.HasOne(e => e.Theme)
            	.WithMany(t => t.Events)
            	.HasForeignKey(e => e.ThemeId)
            	.OnDelete(DeleteBehavior.Restrict);
        });
    }
}