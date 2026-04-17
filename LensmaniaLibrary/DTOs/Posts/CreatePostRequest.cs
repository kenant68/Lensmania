using System.ComponentModel.DataAnnotations;

namespace LensmaniaLibrary.DTOs.Posts;

public class CreatePostRequest
{
    [MaxLength(150)]
    public string? Title { get; set; }

    [Required] 
    public string PhotoUrl { get; set; } = string.Empty;
    
    [MaxLength(300)]
    public string? Description { get; set; }
}
