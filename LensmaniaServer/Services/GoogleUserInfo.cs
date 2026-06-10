namespace LensmaniaServer.Services;

public record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified,
    string? Name,
    string? Picture);
