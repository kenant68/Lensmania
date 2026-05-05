using Microsoft.EntityFrameworkCore;
using LensmaniaServer.Models;
using LensmaniaLibrary.DTOs.Users;
using LensmaniaServer.Database;

namespace LensmaniaServer.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
	private readonly IWebHostEnvironment _env;

    public UserService(AppDbContext db, IWebHostEnvironment env)
	{
		_db = db;
		_env = env;
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
