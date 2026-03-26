using LensmaniaServer.Models;
using LensmaniaServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AuthService _auth;
    public AuthController(AuthService auth) { _auth = auth; }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req) {
        try
        {
            var result = await _auth.Register(req);
            return result is null
                ? Conflict(new ApiErrorResponse(
                    AuthErrorCodes.DuplicateIdentity,
                    "Email or username is already in use."))
                : Ok(result);
        }
        catch (ApiConflictException ex)
        {
            return Conflict(new ApiErrorResponse(
                AuthErrorCodes.DuplicateIdentity,
                ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req) {
        var result = await _auth.Login(req);
        return result is null
            ? Unauthorized(new ApiErrorResponse(
                AuthErrorCodes.InvalidCredentials,
                "Invalid email or password."))
            : Ok(result);
    }
}