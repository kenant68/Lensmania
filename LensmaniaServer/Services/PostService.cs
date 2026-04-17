using Microsoft.EntityFrameworkCore;
using LensmaniaServer.Models;
using LensmaniaLibrary.DTOs.Posts;
using LensmaniaServer.Database;

namespace LensmaniaServer.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _db;

    public PostService(AppDbContext db) => _db = db;
    
    public async Task<PaginatedPosts> GetAllAsync(int? cursor, int limit)
    {
        var query = _db.Posts
            .OrderByDescending(p => p.Id)
            .AsQueryable();

        if (cursor.HasValue)
        {
            query = query.Where(p => p.Id < cursor.Value);
        }
        
        var posts = await query
            .Select(p => new PostListItemResponse(
                p.Id,
                p.Title ?? $"Photo de {p.User.Username}",
                p.PhotoUrl,
                p.User.Username
            ))
            .Take(limit + 1)
            .ToListAsync();

        var hasMore = posts.Count > limit;
        var items = hasMore ? posts.Take(limit).ToList() : posts;

        return new PaginatedPosts
        {
            Posts = items,
            HasMore = hasMore,
            NextCursor = hasMore ? items.Last().Id : null
        };
    }

    public async Task<PostResponse?> GetByIdAsync(int id)
    {
        return await _db.Posts
            .Where(p => p.Id == id)
            .Select(p => new PostResponse(
                p.Id,
                p.Title,
                p.PhotoUrl,
                p.Description,
                p.CreatedAt,
                p.User.Username
            ))
            .FirstOrDefaultAsync();
    }
    
    public async Task<PostListItemResponse> CreatePostAsync(CreatePostRequest request, int  userId)
    {
        if (string.IsNullOrWhiteSpace(request.PhotoUrl))
            throw new ArgumentException("PhotoUrl is required");

		// PhotoUrl check : only accept paths from our upload endpoint
		if (!request.PhotoUrl.StartsWith("/uploads/photos/", StringComparison.Ordinal)
			|| request.PhotoUrl.Contains("..", StringComparison.Ordinal))
			throw new ArgumentException("PhotoUrl must reference an uploaded photo");
        
        var post = new Post
        {
            Title = request.Title,
            PhotoUrl = request.PhotoUrl,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        
        await _db.Entry(post)
            .Reference(p => p.User)
            .LoadAsync();

        return new PostListItemResponse(
            post.Id,
            post.Title ?? $"Photo de {post.User.Username}",
            post.PhotoUrl,
            post.User.Username
        );
    }
}
