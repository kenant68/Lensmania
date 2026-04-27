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
    private readonly AppDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly PasswordResetOptions _options;

    public PasswordResetService(AppDbContext db, IEmailSender emailSender, IOptions<PasswordResetOptions> options)
    {
        _db = db;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public Task RequestResetAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<ResetOutcome> ResetAsync(string token, string newPassword)
    {
        throw new NotImplementedException();
    }
}