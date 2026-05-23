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
    
    public async Task<PublicUserProfileResponse?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) 
            return null;
        
        return await _db.Users
            .Where(u => u.Username.ToLower() == username.ToLower())
            .Select(u => new PublicUserProfileResponse(
                u.Id,
                u.Username
            ))
            .FirstOrDefaultAsync();
    }
    
    public async Task<PaginatedUsers> GetAllAsync(int offset, int limit)
    {
        var total = await _db.Users.CountAsync();

        var users = await _db.Users
            .OrderBy(u => u.Id)
            .Skip(offset)
            .Take(limit)
            .Select(u => new UserAdminResponse(
                u.Id, 
                u.Username, 
                u.Email, 
                u.IsActive, 
                u.IsAdmin, 
                u.IsPremium, 
                u.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedUsers
        {
            Users = users,
            Total = total,
            Offset = offset,
            Limit = limit
        };
    }
    
    public async Task<UserResponse?> UpdateAsync(int userId, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        
        if (user is null || !user.IsActive) 
            return null;

        if (!string.IsNullOrWhiteSpace(request.Username) &&
            request.Username != user.Username)
        {
            var taken = await _db.Users
                .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower() && u.Id != userId);
            if (taken)
                throw new ArgumentException("Ce nom d'utilisateur est déjà pris.");

            user.Username = request.Username.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            request.Email != user.Email)
        {
            var taken = await _db.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != userId);
            if (taken)
                throw new ArgumentException("Cet email n'est pas disponible.");

            user.Email = request.Email.Trim();
        }

        await _db.SaveChangesAsync();
        return new UserResponse(user.Id, user.Username, user.Email);
    }
    
    public async Task<bool> DeleteByAdminAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        
        if (user is null) 
            return false;

        if (user.IsAdmin)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer un administrateur.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> DeleteMeAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        
        if (user is null) 
            return false;
        
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> ToggleIsActiveAsync(int userId, bool isActive)
    {
        var user = await _db.Users.FindAsync(userId);
        
        if (user is null) 
            return false;

        user.IsActive = isActive;
        await _db.SaveChangesAsync();
        
        return true;
    }

	public async Task<bool> IsActiveAsync(int userId)
	{
    	return await _db.Users
        	.Where(u => u.Id == userId)
        	.Select(u => u.IsActive)
        	.FirstOrDefaultAsync();
	}
}
