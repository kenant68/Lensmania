using LensmaniaLibrary.DTOs.Users;

namespace LensmaniaServer.Services;

public interface IUserService
{
    Task<UserResponse?> GetByUsernameAsync(string username);
}
