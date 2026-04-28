using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaServer.Services;

public interface IPostService
{
    Task<PaginatedPosts> GetAllAsync(int? cursor, int limit);
    Task<PostResponse?> GetByIdAsync(int id);
    Task<PostResponse> CreatePostAsync(CreatePostRequest request, int userId);
    Task<bool> DeletePostAsync(int postId, int currentUserId);
}
