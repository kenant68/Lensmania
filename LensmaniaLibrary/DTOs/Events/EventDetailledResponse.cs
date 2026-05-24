using LensmaniaLibrary.DTOs.Themes;
using LensmaniaLibrary.DTOs.Badges;

namespace LensmaniaLibrary.DTOs.Events;

public record EventDetailledResponse(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPremium,
    ThemeResponse Theme,
    List<BadgeResponse> Badges
);
