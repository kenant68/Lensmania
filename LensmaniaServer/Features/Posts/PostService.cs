using Microsoft.EntityFrameworkCore;
using LensmaniaLibrary.Models;
using LensmaniaServer.Database;

namespace LensmaniaServer.Features.Posts;

public class PostService : IPostService
{
    private readonly AppDbContext _db;

    public PostService(AppDbContext db) => _db = db;

    public async Task<List<Post>> GetAllAsync()
    {
        return await _db.Posts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _db.Posts.FindAsync(id);
    }
}
