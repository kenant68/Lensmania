using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LensmaniaServer.Models;
using Microsoft.IdentityModel.Tokens;

namespace LensmaniaServer.Services;

public class TokenService {
    private readonly IConfiguration _cfg;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly int _expirationMinutes;

    public TokenService(IConfiguration cfg) {
        _cfg = cfg;
        var keyString = cfg["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyString)) {
            throw new InvalidOperationException(
                "Configuration value 'Jwt:Key' is missing or empty. Set a non-empty signing key in configuration.");
        }
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        _expirationMinutes = _cfg.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;
        if (_expirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Configuration value 'Jwt:ExpirationMinutes' must be a positive integer.");
        }
    }

    public string GenerateToken(User user) {
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("isAdmin", user.IsAdmin.ToString()),
            new Claim("isPremium", user.IsPremium.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
