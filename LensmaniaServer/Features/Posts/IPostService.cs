using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Features.Posts;

public interface IPostService
{
    Task<PaginatedPosts> GetAllAsync(int? cursor, int limit);
    Task<PostResponse?> GetByIdAsync(int id);
}
