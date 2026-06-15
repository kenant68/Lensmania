namespace LensmaniaLibrary.DTOs.Themes;

public class PaginatedThemes
{
    public List<ThemeResponse> Themes { get; init; } = new();
    public int Total  { get; init; }
    public int Offset { get; init; }
    public int Limit  { get; init; }
}
