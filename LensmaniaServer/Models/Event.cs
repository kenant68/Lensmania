namespace LensmaniaServer.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPremium { get; set; } = false;
    public int ThemeId { get; set; }
    public int UserId { get; set; }
    public int? CoverPhotoPostId { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? WinnerPostId { get; set; }

    public Theme Theme { get; set; } = null!;
    public User User { get; set; } = null!;
    public Post? CoverPhoto { get; set; }
    public Post? WinnerPost { get; set; }

    public List<Post> Posts { get; } = new();
    public List<Badge> Badges { get; } = new();
}
