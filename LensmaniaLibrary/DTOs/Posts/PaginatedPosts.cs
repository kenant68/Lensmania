namespace LensmaniaLibrary.DTOs.Posts;

public class PaginatedPosts
{
    public List<PostListItemResponse> Posts { get; set; } = [];
    public bool HasMore { get; set; }
    public int? NextCursor { get; set; }
    public int? NextOffset { get; set; }
}
