using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Services;

public interface IPostService
{
    Task<PaginatedPosts> GetAllAsync(int? cursor, int limit);
    Task<PostDto?> GetByIdAsync(int id);
}
