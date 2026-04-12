namespace LensmaniaLibrary.DTOs.Posts;

public class CreatePostRequest
{
    public string? Title { get; set; } = string.Empty;
    public required string PhotoUrl { get; set; }
    public string? Description { get; set; } = string.Empty;
}
