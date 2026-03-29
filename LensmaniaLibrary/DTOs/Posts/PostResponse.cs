namespace LensmaniaLibrary.DTOs.Posts;

public class PostResponse
{
    public string? Title { get; set; } = string.Empty;
    public required string PhotoUrl { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
