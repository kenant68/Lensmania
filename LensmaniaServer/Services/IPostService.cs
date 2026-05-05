using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Services;

public interface IPostService
{
    Task<PaginatedPosts> GetAllAsync(int? userId, int? cursor, int limit, int? currentUserId = null);
    Task<PostResponse?> GetByIdAsync(int id);
    Task<PostResponse> CreatePostAsync(CreatePostRequest request, int userId);
    Task<bool> DeletePostAsync(int postId, int currentUserId);
    Task<bool> ToggleLikeAsync(int postId, int userId);
}
