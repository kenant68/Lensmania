namespace LensmaniaLibrary.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPremium { get; set; }
    public int ThemeId { get; set; }

    // Navigation properties
    public Theme Theme { get; set; } = null!;
}