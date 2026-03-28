using Microsoft.EntityFrameworkCore;
using LensmaniaLibrary.Models;
using LensmaniaServer.Database;
using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Features.Posts;

public class PostService : IPostService
{
    private readonly AppDbContext _db;

    public PostService(AppDbContext db) => _db = db;
    
    public async Task<PaginatedPosts> GetAllAsync(int? cursor, int limit)
    {
        var query = _db.Posts
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
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
        
        var itemDtos = items.Select(p => new PostResponse
        {
            Title = p.Title,
            PhotoUrl = p.PhotoUrl,
            Description = p.Description,
            CreatedAt = p.CreatedAt
        }).ToList();

        return new PaginatedPosts
        {
            Posts = itemDtos,
            HasMore = hasMore,
            NextCursor = hasMore ? items.Last().Id : null
        };
    }

    public async Task<PostResponse?> GetByIdAsync(int id)
    {
        var post = await _db.Posts.FindAsync(id);

    	if (post == null)
        	return null;

    	return new PostResponse
    	{
        	Title = post.Title,
        	PhotoUrl = post.PhotoUrl,
			Description = post.Description,
        	CreatedAt = post.CreatedAt
    	};
    }
}
