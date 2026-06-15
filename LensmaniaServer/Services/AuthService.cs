using LensmaniaServer.Database;
using LensmaniaServer.Exceptions;
using LensmaniaServer.Models;
using LensmaniaLibrary.Enums;
using LensmaniaLibrary.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LensmaniaServer.Services;

public class AuthService {
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    public AuthService(AppDbContext db, TokenService tokens) {
        _db = db; 
		_tokens = tokens;
    }

    public async Task<AuthResponse?> Register(RegisterRequest req) 
	{
        if (await _db.Users.AnyAsync(u => u.Email == req.Email || u.Username == req.Username)) return null;
        var user = new User {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var pg = ex.InnerException as PostgresException ?? ex.GetBaseException() as PostgresException;
            if (pg != null && pg.SqlState == "23505")
                throw new ApiConflictException("Email ou nom d'utilisateur déjà utilisé.");
            throw;
        }
        return new AuthResponse(_tokens.GenerateToken(user), user.Username, user.IsAdmin, user.IsPremium, user.IsActive);
    }

    public async Task<(LoginStatus Status, AuthResponse? Response)> Login(LoginRequest req) 
	{
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        
		if (user is null ||
		    user.PasswordHash is null ||
		    !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
		{
			return (LoginStatus.InvalidCredentials, null);
		}

		if (!user.IsActive)
			return (LoginStatus.Blocked, null);
        
		return (
			LoginStatus.Success,
			new AuthResponse(_tokens.GenerateToken(user), user.Username, user.IsAdmin, user.IsPremium, user.IsActive)
		);
    }
}
