namespace LensmaniaLibrary.DTOs.Posts;

public record PostListItemResponse(
    int Id,
    string Title,
    string PhotoUrl,
    string Username
);
