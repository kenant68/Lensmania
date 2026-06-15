namespace LensmaniaServer.Models;

public class Earn
{
    public int UserId { get; set; }
    public int BadgeId { get; set; }
    public DateTime AwardedAt { get; set; }


    public User User { get; set; } = null!;
    public Badge Badge { get; set; } = null!;
}
