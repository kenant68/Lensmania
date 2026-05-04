using LensmaniaServer.Models;
using LensmaniaServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private const string ForgotPasswordGenericMessage =
        "Si l'adresse correspond à un compte, un lien de réinitialisation a été envoyé.";
    private const string ResetPasswordSuccessMessage =
        "Mot de passe modifié avec succès.";
    private const string InvalidOrExpiredMessage =
        "Ce lien de réinitialisation est invalide ou a expiré.";

    private readonly AuthService _auth;
    private readonly PasswordResetService _passwordReset;

    public AuthController(AuthService auth, PasswordResetService passwordReset)
    {
        _auth = auth;
        _passwordReset = passwordReset;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req) {
        try
        {
            var result = await _auth.Register(req);
            return result is null
                ? Conflict(new ApiErrorResponse(
                    AuthErrorCodes.DuplicateIdentity,
                    "Cet e-mail ou ce nom d'utilisateur est deja utilise."))
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
                "E-mail ou mot de passe incorrect."))
            : Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        await _passwordReset.RequestResetAsync(req.Email);
        return Ok(new SimpleMessageResponse(ForgotPasswordGenericMessage));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        var outcome = await _passwordReset.ResetAsync(req.Token, req.NewPassword);
        return outcome switch
        {
            ResetOutcome.Success => Ok(new SimpleMessageResponse(ResetPasswordSuccessMessage)),
            _ => BadRequest(new ApiErrorResponse(AuthErrorCodes.InvalidOrExpiredResetToken, InvalidOrExpiredMessage))
        };
    }
}