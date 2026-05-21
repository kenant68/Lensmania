using LensmaniaLibrary.DTOs.Users;

namespace LensmaniaServer.Services;

public interface IUserService
{
    Task<UserResponse?> GetByUsernameAsync(string username);
    Task<PaginatedUsers> GetAllAsync(int offset, int limit);
    Task<UserResponse?> UpdateAsync(int userId, UpdateUserRequest request);
    Task<bool> DeleteByAdminAsync(int userId);
    Task<bool> DeleteMeAsync(int userId);
    Task<bool> ToggleIsActiveAsync(int userId, bool isActive);
}
