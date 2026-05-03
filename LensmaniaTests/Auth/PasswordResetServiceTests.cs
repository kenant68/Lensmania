using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LensmaniaTests.Auth;

[TestFixture]
public class PasswordResetServiceTests
{
    private AppDbContext _db = null!;
    private FakeEmailSender _emailSender = null!;
    private PasswordResetService _service = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _emailSender = new FakeEmailSender();
        var options = Options.Create(new PasswordResetOptions
        {
            TokenLifetimeMinutes = 60,
            ClientBaseUrl = "https://localhost:5135"
        });
        _service = new PasswordResetService(_db,_emailSender,options);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }
    
    private async Task<User> SeedUserAsync(string email = "john@example.com")
    {
        var user = new User
        {
            Username = "john",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    [Test]
    public async Task RequestResetAsync_UserExists_GeneratesTokenAndSendsEmail()
    {
        var user = await SeedUserAsync();

        await _service.RequestResetAsync(user.Email);

        Assert.That(_db.PasswordResetTokens.Count(), Is.EqualTo(1));
        Assert.That(_emailSender.SentEmails, Has.Count.EqualTo(1));
        Assert.That(_emailSender.SentEmails[0].To, Is.EqualTo(user.Email));
        Assert.That(_emailSender.SentEmails[0].HtmlBody, Does.Contain("/reset-password?token="));
    }

    [Test]
    public async Task RequestResetAsync_UserDoesNotExist_DoesNotSendEmail_DoesNotThrow()
    {
        Assert.DoesNotThrowAsync(async () =>
            await _service.RequestResetAsync("ghost@example.com"));

        Assert.That(_db.PasswordResetTokens.Count(), Is.EqualTo(0));
        Assert.That(_emailSender.SentEmails, Is.Empty);
    }

    [Test]
    public async Task RequestResetAsync_PreviousUnconsumedTokensAreInvalidated()
    {
        var user = await SeedUserAsync();

        await _service.RequestResetAsync(user.Email);
        await _service.RequestResetAsync(user.Email);

        var tokens = await _db.PasswordResetTokens.AsNoTracking().ToListAsync();
        Assert.That(tokens, Has.Count.EqualTo(2));
        Assert.That(tokens.Count(t => t.ConsumedAt == null), Is.EqualTo(1));
    }
}