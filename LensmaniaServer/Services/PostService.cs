using Microsoft.EntityFrameworkCore;
using LensmaniaLibrary.Models;
using LensmaniaServer.Database;
using LensmaniaLibrary.DTOs.Posts;

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
			.Take(limit + 1)
			.ToListAsync();

        var hasMore = posts.Count > limit;
        var items = hasMore ? posts.Take(limit).ToList() : posts;
        
        var itemDtos = items.Select(p => new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            PhotoUrl = p.PhotoUrl,
            Description = p.Description,
            CreatedAt = p.CreatedAt
        }).ToList();

        return new PaginatedPosts
        {
            Posts = itemDtos,
            HasMore = hasMore,
            NextCursor = hasMore && items.Any() ? items.Last().Id : null
        };
    }

    public async Task<PostDto?> GetByIdAsync(int id)
    {
        var post = await _db.Posts.FindAsync(id);

    	if (post == null)
        	return null;

    	return new PostDto
    	{
        	Id = post.Id,
            Title = post.Title,
        	PhotoUrl = post.PhotoUrl,
			Description = post.Description,
        	CreatedAt = post.CreatedAt
    	};
    }
    
    public async Task<PostDto> CreatePostAsync(CreatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhotoUrl))
            throw new ArgumentException("PhotoUrl is required");
        
        var post = new Post
        {
            Title = request.Title,
            PhotoUrl = request.PhotoUrl,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            PhotoUrl = post.PhotoUrl,
            Description = post.Description,
            CreatedAt = post.CreatedAt
        };
    }
}
