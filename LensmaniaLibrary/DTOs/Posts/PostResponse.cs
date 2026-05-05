namespace LensmaniaLibrary.DTOs.Posts;

public record PostResponse (
    int Id,
    string? Title,
    string PhotoUrl,
    string? Description,
    DateTime CreatedAt,
    string Username,
    int UserId
);
