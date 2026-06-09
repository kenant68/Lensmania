using System.ComponentModel.DataAnnotations;

namespace LensmaniaServer.Services;

public class EventClosingOptions
{
    [Range(1, 86400)]
    public int IntervalSeconds { get; set; } = 60;
}
