namespace LensmaniaServer.Models;

public class Badge
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int EventId { get; set; }
    
    public Event Event { get; set; } = null!;
    public List<Earn> Earns { get; } = new();
}
