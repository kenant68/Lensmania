namespace LensmaniaServer.Models;

public static class AuthErrorCodes
{
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string DuplicateIdentity = "AUTH_DUPLICATE_IDENTITY";
    public const string ValidationFailed = "AUTH_VALIDATION_FAILED";
    public const string InvalidOrExpiredResetToken = "AUTH_INVALID_OR_EXPIRED_RESET_TOKEN";
    public const string UserIsBlocked = "AUTH_USER_IS_BLOCKED";
    public const string GoogleInvalidToken = "AUTH_GOOGLE_INVALID_TOKEN";
    public const string GoogleEmailUnverified = "AUTH_GOOGLE_EMAIL_UNVERIFIED";
}
