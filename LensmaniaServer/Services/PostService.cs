using Microsoft.EntityFrameworkCore;
using LensmaniaServer.Models;
using LensmaniaLibrary.DTOs.Posts;
using LensmaniaServer.Database;

namespace LensmaniaServer.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _db;
	private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png"];
	private const string DefaultAltImgPrefix = "Photo de ";
    private static string DefaultAltImgFor(string username) => DefaultAltImgPrefix + username;

    public PostService(AppDbContext db, IWebHostEnvironment  env)
	{
		_db = db;
		_env = env;
	}
    
    public async Task<PaginatedPosts> GetAllAsync(int? userId, int? cursor, int limit)
    {
        var query = _db.Posts.AsQueryable();

		// Filter by userId and cursor
        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (cursor.HasValue)
        {
            query = query.Where(p => p.Id < cursor.Value);
        }
        
        var posts = await query
            .OrderByDescending(p => p.Id)
            .Select(p => new PostListItemResponse(
                p.Id,
                p.Title ?? DefaultAltImgFor(p.User.Username),
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
                p.User.Username,
                p.UserId
            ))
            .FirstOrDefaultAsync();
    }
    
    public async Task<PostResponse> CreatePostAsync(CreatePostRequest request, int  userId)
    {
        if (string.IsNullOrWhiteSpace(request.PhotoUrl))
            throw new ArgumentException("PhotoUrl is required");

        // Reject any path separators: the value must be a bare filename
        if (request.PhotoUrl.Contains('/') || request.PhotoUrl.Contains('\\'))
            throw new ArgumentException("PhotoUrl must be a filename, not a path");

        // Only allow extensions produced by the upload endpoint
        var ext = Path.GetExtension(request.PhotoUrl).ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(ext))
            throw new ArgumentException("PhotoUrl has an invalid extension");

        // Resolve to an absolute path and confirm it stays inside the uploads folder
        var uploadsFolder = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads", "photos"));
        var fullPath = Path.GetFullPath(Path.Combine(uploadsFolder, request.PhotoUrl));

        if (!fullPath.StartsWith(uploadsFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !fullPath.Equals(uploadsFolder, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("PhotoUrl resolves outside the uploads directory");

        // Verify the file was actually uploaded
        if (!File.Exists(fullPath))
            throw new ArgumentException("The referenced photo does not exist on the server");
        
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

        return new PostResponse(
            post.Id,
            post.Title ?? DefaultAltImgFor(post.User.Username),
            post.PhotoUrl,
            post.Description,
            post.CreatedAt,
            post.User.Username,
            post.UserId
        );
    }
}
