using LensmaniaServer.Controllers;
using LensmaniaServer.Database;
using LensmaniaServer.Models;
using LensmaniaServer.Services;
using LensmaniaServer.Errors;
using LensmaniaServer.Options;
using LensmaniaLibrary.DTOs.Auth;
using LensmaniaLibrary.DTOs;
using LensmaniaTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LensmaniaTests.Auth;

[TestFixture]
public class AuthControllerPasswordResetTests
{
    private AppDbContext _db = null!;
    private FakeEmailSender _emailSender = null!;
    private PasswordResetService _passwordReset = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _db = TestHelpers.CreateInMemoryDb();
        _emailSender = new FakeEmailSender();
        var resetOptions = Options.Create(new PasswordResetOptions
        {
            TokenLifetimeMinutes = 60,
            ClientBaseUrl = "http://localhost:5135"
        });
        _passwordReset = new PasswordResetService(_db, _emailSender, NullLogger<PasswordResetService>.Instance, resetOptions);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-test-key-test-key-test-key-test-key-test",
                ["Jwt:Issuer"] = "Test",
                ["Jwt:Audience"] = "Test",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
        var tokens = new TokenService(configuration);
        var auth = new AuthService(_db, tokens);

        _controller = new AuthController(auth, _passwordReset);
    }

    [TearDown]
    public void Teardown()
    {
        _db.Dispose();
    }

    private async Task<User> SeedUserAsync(string email = "alice@example.com")
    {
        var user = new User
        {
            Username = "alice",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    [Test]
    public async Task ForgotPassword_AlwaysReturns200_RegardlessOfEmailExistence()
    {
        await SeedUserAsync();

        var existing = await _controller.ForgotPassword(new ForgotPasswordRequest("alice@example.com"));
        var unknown = await _controller.ForgotPassword(new ForgotPasswordRequest("ghost@example.com"));

        Assert.That(existing, Is.InstanceOf<OkObjectResult>());
        Assert.That(unknown, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task ResetPassword_ValidRequest_Returns200()
    {
        var user = await SeedUserAsync();
        await _passwordReset.RequestResetAsync(user.Email);
        var sent = _emailSender.SentEmails.Last();
        var marker = "?token=";
        var startIndex = sent.HtmlBody.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var endIndex = sent.HtmlBody.IndexOf("\"", startIndex, StringComparison.Ordinal);
        var rawToken = sent.HtmlBody.Substring(startIndex, endIndex - startIndex);

        var result = await _controller.ResetPassword(new ResetPasswordRequest(rawToken, "newpassword123"));

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task ResetPassword_InvalidToken_Returns400_WithInvalidOrExpiredCode()
    {
        await SeedUserAsync();

        var result = await _controller.ResetPassword(new ResetPasswordRequest("nope", "newpassword123"));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result;
        Assert.That(badRequest.Value, Is.InstanceOf<ApiErrorResponse>());
        var error = (ApiErrorResponse)badRequest.Value!;
        Assert.That(error.Code, Is.EqualTo(AuthErrorCodes.InvalidOrExpiredResetToken));
    }
}