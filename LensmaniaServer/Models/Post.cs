namespace LensmaniaServer.Models;

public class Post
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public required string PhotoUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int LikesCount { get; set; }
    public int UserId { get; set; }
	public int? EventId { get; set; }

    public User User { get; set; } = null!;
	public Event? Event { get; set; } = null!;
    public List<PostLike> Likes { get; } = new();
}
