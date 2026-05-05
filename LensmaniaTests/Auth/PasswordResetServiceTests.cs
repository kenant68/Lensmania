using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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
        _service = new PasswordResetService(_db, _emailSender, NullLogger<PasswordResetService>.Instance, options);
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
    
    
    private async Task<string> RequestAndCaptureRawTokenAsync(User user)
{
    await _service.RequestResetAsync(user.Email);
    var sent = _emailSender.SentEmails.Last();
    var marker = "?token=";
    var startIndex = sent.HtmlBody.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
    var endIndex = sent.HtmlBody.IndexOf("\"", startIndex, StringComparison.Ordinal);
    return sent.HtmlBody.Substring(startIndex, endIndex - startIndex);
}

    [Test]
    public async Task ResetAsync_ValidToken_UpdatesPasswordAndConsumesToken()
    {
        var user = await SeedUserAsync();
        var rawToken = await RequestAndCaptureRawTokenAsync(user);

        var outcome = await _service.ResetAsync(rawToken, "newpassword123");

        Assert.That(outcome, Is.EqualTo(ResetOutcome.Success));

        var refreshed = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
        Assert.That(BCrypt.Net.BCrypt.Verify("newpassword123", refreshed.PasswordHash), Is.True);

        var dbToken = await _db.PasswordResetTokens.AsNoTracking().FirstAsync();
        Assert.That(dbToken.ConsumedAt, Is.Not.Null);
    }

    [Test]
    public async Task ResetAsync_ExpiredToken_ReturnsInvalidOrExpired_PasswordUnchanged()
    {
        var user = await SeedUserAsync();
        var rawToken = await RequestAndCaptureRawTokenAsync(user);

        var stored = await _db.PasswordResetTokens.FirstAsync();
        stored.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await _db.SaveChangesAsync();

        var outcome = await _service.ResetAsync(rawToken, "newpassword123");

        Assert.That(outcome, Is.EqualTo(ResetOutcome.InvalidOrExpired));

        var refreshed = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
        Assert.That(BCrypt.Net.BCrypt.Verify("oldpassword", refreshed.PasswordHash), Is.True);
    }

    [Test]
    public async Task ResetAsync_AlreadyConsumedToken_ReturnsInvalidOrExpired()
    {
        var user = await SeedUserAsync();
        var rawToken = await RequestAndCaptureRawTokenAsync(user);

        var first = await _service.ResetAsync(rawToken, "newpassword123");
        Assert.That(first, Is.EqualTo(ResetOutcome.Success));

        var second = await _service.ResetAsync(rawToken, "anothernewpassword");
        Assert.That(second, Is.EqualTo(ResetOutcome.InvalidOrExpired));
    }

    [Test]
    public async Task ResetAsync_UnknownToken_ReturnsInvalidOrExpired()
    {
        await SeedUserAsync();

        var outcome = await _service.ResetAsync("totally-not-a-real-token", "newpassword123");

        Assert.That(outcome, Is.EqualTo(ResetOutcome.InvalidOrExpired));
    }

    [Test]
    public async Task ResetAsync_Success_SendsConfirmationEmail()
    {
        var user = await SeedUserAsync();
        var rawToken = await RequestAndCaptureRawTokenAsync(user);

        var sentBefore = _emailSender.SentEmails.Count;
        await _service.ResetAsync(rawToken, "newpassword123");

        Assert.That(_emailSender.SentEmails.Count, Is.EqualTo(sentBefore + 1));
        Assert.That(_emailSender.SentEmails.Last().Subject, Does.Contain("modifié"));
        Assert.That(_emailSender.SentEmails.Last().To, Is.EqualTo(user.Email));
    }
}