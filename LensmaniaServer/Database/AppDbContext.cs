using LensmaniaServer.Models;
using LensmaniaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Database;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Username)
            .IsUnique();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
}