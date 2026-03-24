namespace LensmaniaServer.Models;

public record AuthResponse(string Token, string Username, bool IsAdmin, bool IsPremium);