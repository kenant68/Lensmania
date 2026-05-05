namespace LensmaniaClient.Models.Auth;

public record RegisterRequestDto(string Username, string Email, string Password);

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(string Token, string Username, bool IsAdmin, bool IsPremium);

public record ApiErrorDto(string Code, string Message, Dictionary<string, string[]>? Errors = null);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(string Token, string NewPassword);

public record SimpleMessageResponseDto(string Message);