using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LensmaniaTests.Auth;

[TestFixture]
public class GoogleAuthServiceTests
{
    private AppDbContext _db = null!;
    private FakeGoogleTokenValidator _validator = null!;
    private GoogleAuthService _service = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _validator = new FakeGoogleTokenValidator();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-at-least-32-bytes-long!!",
                ["Jwt:Issuer"] = "LensmaniaServer",
                ["Jwt:Audience"] = "LensmaniaClient",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
        var tokens = new TokenService(cfg);

        _service = new GoogleAuthService(_db, tokens, _validator);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private static GoogleUserInfo Info(
        string sub = "google-sub-1",
        string email = "alice@example.com",
        bool verified = true,
        string? name = "Alice")
        => new(sub, email, verified, name, null);

    [Test]
    public async Task SignIn_InvalidToken_ReturnsInvalidToken()
    {
        _validator.Result = null;

        var (status, response) = await _service.SignInAsync("bad-token");

        Assert.That(status, Is.EqualTo(GoogleAuthStatus.InvalidToken));
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task SignIn_EmailNotVerified_ReturnsEmailUnverified_AndCreatesNothing()
    {
        _validator.Result = Info(verified: false);

        var (status, response) = await _service.SignInAsync("token");

        Assert.That(status, Is.EqualTo(GoogleAuthStatus.EmailUnverified));
        Assert.That(response, Is.Null);
        Assert.That(_db.Users.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task SignIn_ExistingUserByGoogleId_ReturnsSuccess()
    {
        _db.Users.Add(new User
        {
            Username = "alice", Email = "alice@example.com",
            GoogleId = "google-sub-1", AuthProvider = AuthProvider.Google
        });
        await _db.SaveChangesAsync();
        _validator.Result = Info();

        var (status, response) = await _service.SignInAsync("token");

        Assert.That(status, Is.EqualTo(GoogleAuthStatus.Success));
        Assert.That(response!.Username, Is.EqualTo("alice"));
        Assert.That(_db.Users.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task SignIn_ExistingUserByVerifiedEmail_LinksGoogleId()
    {
        _db.Users.Add(new User
        {
            Username = "alice", Email = "alice@example.com",
            PasswordHash = "hash", AuthProvider = AuthProvider.Local
        });
        await _db.SaveChangesAsync();
        _validator.Result = Info();

        var (status, _) = await _service.SignInAsync("token");

        Assert.That(status, Is.EqualTo(GoogleAuthStatus.Success));
        var user = _db.Users.Single();
        Assert.That(user.GoogleId, Is.EqualTo("google-sub-1"));
    }

    [Test]
    public async Task SignIn_NewUser_CreatesAccountWithDerivedUsername()
    {
        _validator.Result = Info(email: "newperson@example.com", name: "New Person");

        var (status, response) = await _service.SignInAsync("token");

        Assert.That(status, Is.EqualTo(GoogleAuthStatus.Success));
        var user = _db.Users.Single();
        Assert.That(user.GoogleId, Is.EqualTo("google-sub-1"));
        Assert.That(user.PasswordHash, Is.Null);
        Assert.That(user.AuthProvider, Is.EqualTo(AuthProvider.Google));
        Assert.That(user.Username, Is.EqualTo("newperson"));
        Assert.That(response!.Username, Is.EqualTo("newperson"));
    }

    [Test]
    public async Task SignIn_NewUser_UsernameCollision_AppendsSuffix()
    {
        _db.Users.Add(new User { Username = "alice", Email = "other@example.com" });
        await _db.SaveChangesAsync();
        _validator.Result = Info(email: "alice2@example.com", name: "Alice");

        await _service.SignInAsync("token");

        var created = _db.Users.Single(u => u.Email == "alice2@example.com");
        Assert.That(created.Username, Is.EqualTo("alice2"));
    }

    [Test]
    public async Task SignIn_BlockedUser_ReturnsBlocked()
    {
        _db.Users.Add(new User
        {
            Username = "alice", Email = "alice@example.com",
            GoogleId = "google-sub-1", IsActive = false
        });
        await _db.SaveChangesAsync();
        _validator.Result = Info();

        var (status, response) = await _service.SignInAsync("token");

        Assert.That(status, Is.EqualTo(GoogleAuthStatus.Blocked));
        Assert.That(response, Is.Null);
    }
}
