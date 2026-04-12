namespace LensmaniaLibrary.DTOs.Posts;

public class PaginatedPosts
{
    public List<PostDto> Posts { get; set; } = [];
    public int? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
