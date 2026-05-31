using LensmaniaServer.Database;
using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LensmaniaServer.Services;

public class GoogleAuthService
{
    private const int UsernameMaxLength = 32;
    private const int UsernameMinLength = 3;

    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly IGoogleTokenValidator _validator;

    public GoogleAuthService(AppDbContext db, TokenService tokens, IGoogleTokenValidator validator)
    {
        _db = db;
        _tokens = tokens;
        _validator = validator;
    }

    public async Task<(GoogleAuthStatus Status, AuthResponse? Response)> SignInAsync(string idToken)
    {
        var info = await _validator.ValidateAsync(idToken);
        if (info is null)
            return (GoogleAuthStatus.InvalidToken, null);

        if (!info.EmailVerified)
            return (GoogleAuthStatus.EmailUnverified, null);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == info.Subject);
        if (user is null)
        {
            user = await _db.Users.FirstOrDefaultAsync(u => u.Email == info.Email);
            if (user is not null)
            {
                user.GoogleId = info.Subject;
                await _db.SaveChangesAsync();
            }
            else
            {
                user = await CreateUserAsync(info);
            }
        }

        if (!user.IsActive)
            return (GoogleAuthStatus.Blocked, null);

        var response = new AuthResponse(
            _tokens.GenerateToken(user),
            user.Username,
            user.IsAdmin,
            user.IsPremium,
            user.IsActive);
        return (GoogleAuthStatus.Success, response);
    }

    private async Task<User> CreateUserAsync(GoogleUserInfo info)
    {
        var user = new User
        {
            Username = await GenerateUniqueUsernameAsync(info.Name, info.Email),
            Email = info.Email,
            GoogleId = info.Subject,
            PasswordHash = null,
            AuthProvider = AuthProvider.Google,
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private async Task<string> GenerateUniqueUsernameAsync(string? name, string email)
    {
        var baseName = Normalize(name);
        if (baseName.Length < UsernameMinLength)
            baseName = Normalize(email.Split('@')[0]);
        if (baseName.Length < UsernameMinLength)
            baseName = "user" + baseName;
        if (baseName.Length > UsernameMaxLength)
            baseName = baseName[..UsernameMaxLength];

        var candidate = baseName;
        var suffix = 2;
        while (await _db.Users.AnyAsync(u => u.Username == candidate))
        {
            var suffixText = suffix.ToString();
            var maxBase = UsernameMaxLength - suffixText.Length;
            var trimmed = baseName.Length > maxBase ? baseName[..maxBase] : baseName;
            candidate = trimmed + suffixText;
            suffix++;
        }
        return candidate;
    }

    private static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        return new string(input.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    }
}
