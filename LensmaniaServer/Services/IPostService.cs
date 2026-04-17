using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Services;

public interface IPostService
{
    Task<PaginatedPosts> GetAllAsync(int? cursor, int limit);
    Task<PostResponse?> GetByIdAsync(int id);
    Task<PostListItemResponse> CreatePostAsync(CreatePostRequest request, int userId);
}
