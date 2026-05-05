namespace LensmaniaServer.Models;

public class PostLike
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}
