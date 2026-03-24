using LensmaniaServer.Database;
using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Services;

public class AuthService {
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    public AuthService(AppDbContext db, TokenService tokens) {
        _db = db; _tokens = tokens;
    }

    public async Task<AuthResponse?> Register(RegisterRequest req) {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email)) return null;
        var user = new User {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return new AuthResponse(_tokens.GenerateToken(user), user.Username, user.IsAdmin, user.IsPremium);
    }

    public async Task<AuthResponse?> Login(LoginRequest req) {
        var user = _db.Users.FirstOrDefault(u => u.Email == req.Email);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return null;
        return new AuthResponse(_tokens.GenerateToken(user), user.Username, user.IsAdmin, user.IsPremium);
    }
}