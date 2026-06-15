namespace LensmaniaServer.Models;

public class Theme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }

    public List<Event> Events { get; } = new();
}
