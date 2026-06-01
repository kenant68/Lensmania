using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaLibrary.Enums;
using LensmaniaTests.Helpers;
using Microsoft.Extensions.Configuration;

namespace LensmaniaTests.Auth;

[TestFixture]
public class AuthServiceLoginTests
{
    private static TokenService BuildTokenService()
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-at-least-32-bytes-long!!",
                ["Jwt:Issuer"] = "LensmaniaServer",
                ["Jwt:Audience"] = "LensmaniaClient",
                ["Jwt:ExpirationMinutes"] = "60"
            }).Build();
        return new TokenService(cfg);
    }

    [Test]
    public async Task Login_GoogleOnlyUser_NoPassword_ReturnsInvalidCredentials()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User
        {
            Username = "alice", Email = "alice@example.com",
            PasswordHash = null, GoogleId = "sub-1", AuthProvider = AuthProvider.Google
        });
        await db.SaveChangesAsync();
        var service = new AuthService(db, BuildTokenService());

        var (status, response) = await service.Login(new LoginRequest("alice@example.com", "whatever1"));

        Assert.That(status, Is.EqualTo(LoginStatus.InvalidCredentials));
        Assert.That(response, Is.Null);
    }
}
