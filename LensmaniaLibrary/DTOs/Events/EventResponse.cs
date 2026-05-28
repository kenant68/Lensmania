namespace LensmaniaLibrary.DTOs.Events;

public record EventResponse(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPremium,
    string ThemeName,
    string? ThemeIcon,
    int BadgeCount
);
