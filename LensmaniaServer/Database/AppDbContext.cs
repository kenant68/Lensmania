using LensmaniaServer.Models;
using LensmaniaLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Database;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
}