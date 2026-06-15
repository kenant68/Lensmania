namespace LensmaniaLibrary.DTOs.Auth;

public record AuthResponse(string Token, string Username, bool IsAdmin, bool IsPremium, bool IsActive);
