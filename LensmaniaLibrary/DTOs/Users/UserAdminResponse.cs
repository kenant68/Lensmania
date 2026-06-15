namespace LensmaniaLibrary.DTOs.Users;

public record UserAdminResponse(
    int Id, 
    string Username, 
    string Email, 
    bool IsActive,
    bool IsAdmin,
    bool IsPremium,
    DateTime CreatedAt
);