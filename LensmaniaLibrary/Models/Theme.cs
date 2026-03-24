namespace LensmaniaLibrary.Models;

public class Theme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Event> Events { get; set; } = new List<Event>();
}