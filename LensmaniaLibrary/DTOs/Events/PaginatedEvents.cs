namespace LensmaniaLibrary.DTOs.Events;

public class PaginatedEvents
{
    public List<EventResponse> Events { get; init; } = new();
    public int Total  { get; init; }
    public int Offset { get; init; }
    public int Limit  { get; init; }
}
