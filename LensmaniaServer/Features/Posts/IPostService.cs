using LensmaniaLibrary.Models;

namespace LensmaniaServer.Features.Posts;

public interface IPostService
{
    Task<List<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(int id);
}
