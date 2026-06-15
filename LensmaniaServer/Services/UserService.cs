using Microsoft.EntityFrameworkCore;
using LensmaniaServer.Models;
using LensmaniaLibrary.DTOs.Users;
using LensmaniaServer.Database;
using LensmaniaServer.Exceptions;

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
                u.Username,
                u.Earns
                    .OrderByDescending(e => e.AwardedAt)
                    .Select(e => new UserBadgeResponse(
                        e.BadgeId,
                        e.Badge.Name,
                        e.Badge.ImageUrl,
                        e.Badge.Event.Name,
                        e.AwardedAt,
                        e.Badge.Event.WinnerPost != null ? e.Badge.Event.WinnerPost.PhotoUrl : null))
                    .ToList()
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

        var normalizedUsername = request.Username?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedUsername) &&
            request.Username != user.Username)
        {
            var taken = await _db.Users
                .AnyAsync(u => u.Username.ToLower() == normalizedUsername.ToLower() && u.Id != userId);
            if (taken)
                throw new ArgumentException("Ce nom d'utilisateur est déjà pris.");

            user.Username = normalizedUsername;
        }
        
		var normalizedEmail = request.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            normalizedEmail != user.Email)
        {
            var taken = await _db.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.Id != userId);
            if (taken)
                throw new ArgumentException("Cet email n'est pas disponible.");

            user.Email = normalizedEmail;
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
            throw new BusinessException("Vous n'êtes pas autorisé à supprimer un administrateur.");

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
