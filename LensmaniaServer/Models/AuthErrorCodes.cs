namespace LensmaniaServer.Models;

public static class AuthErrorCodes
{
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string DuplicateIdentity = "AUTH_DUPLICATE_IDENTITY";
    public const string ValidationFailed = "AUTH_VALIDATION_FAILED";
    public const string InvalidOrExpiredResetToken = "AUTH_INVALID_OR_EXPIRED_RESET_TOKEN";
}
