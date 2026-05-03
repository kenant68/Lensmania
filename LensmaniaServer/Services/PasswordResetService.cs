using System.Security.Cryptography;
using System.Text;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LensmaniaServer.Services;

public enum ResetOutcome
{
    Success,
    InvalidOrExpired
}

public class PasswordResetService
{
    private const string ResetSubject = "Réinitialisation de votre mot de passe Lensmania";

    private readonly AppDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly PasswordResetOptions _options;

    public PasswordResetService(AppDbContext db, IEmailSender emailSender, IOptions<PasswordResetOptions> options)
    {
        _db = db;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public async Task RequestResetAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return;

        var existing = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.ConsumedAt == null)
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var t in existing) t.ConsumedAt = now;

        var rawToken = GenerateRawToken();
        var hashed = HashToken(rawToken);

        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hashed,
            ExpiresAt = now.AddMinutes(_options.TokenLifetimeMinutes),
            CreatedAt = now
        });

        await _db.SaveChangesAsync();

        var resetUrl = $"{_options.ClientBaseUrl.TrimEnd('/')}/reset-password?token={rawToken}";
        var body = BuildResetRequestBody(resetUrl);
        await _emailSender.SendAsync(user.Email, ResetSubject, body);
    }

    public Task<ResetOutcome> ResetAsync(string token, string newPassword)
    {
        throw new NotImplementedException();
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashToken(string raw)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }

    private static string BuildResetRequestBody(string resetUrl) =>
        $"""
        <p>Bonjour,</p>
        <p>Vous avez demandé à réinitialiser votre mot de passe. Cliquez sur le lien ci-dessous (valable 1 heure) :</p>
        <p><a href="{resetUrl}">Réinitialiser mon mot de passe</a></p>
        <p>Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer ce message.</p>
        <p>— L'équipe Lensmania</p>
        """;
}