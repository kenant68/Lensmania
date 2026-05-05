namespace LensmaniaClient.Services.Auth;

public enum AuthApiErrorType
{
    None = 0,
    InvalidCredentials,
    DuplicateIdentity,
    ValidationFailed,
    NetworkFailure,
    UnexpectedServerError,
    InvalidOrExpiredResetToken
}

public sealed record AuthApiError(AuthApiErrorType Type, string Message, string? Code = null);

public sealed class AuthApiResult<T>
{
    private AuthApiResult(T? data, AuthApiError? error)
    {
        Data = data;
        Error = error;
    }

    public T? Data { get; }
    public AuthApiError? Error { get; }
    public bool IsSuccess => Error is null;

    public static AuthApiResult<T> Success(T data) => new(data, null);

    public static AuthApiResult<T> Failure(AuthApiError error) => new(default, error);
}
