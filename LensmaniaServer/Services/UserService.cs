using Microsoft.EntityFrameworkCore;
using LensmaniaServer.Models;
using LensmaniaLibrary.DTOs.Users;
using LensmaniaServer.Database;

namespace LensmaniaServer.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
	{
		_db = db;
	}
    
    public async Task<UserResponse?> GetByUsernameAsync(string username)
    {
        return await _db.Users
            .Where(u => u.Username == username)
            .Select(u => new UserResponse(
                u.Id,
                u.Username
            ))
            .FirstOrDefaultAsync();
    }
}
