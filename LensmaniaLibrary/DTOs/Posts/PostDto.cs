namespace LensmaniaLibrary.DTOs.Posts;

public class PostDto
{
    public int Id { get; set; }
    public string? Title { get; set; } = string.Empty;
    public required string PhotoUrl { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
