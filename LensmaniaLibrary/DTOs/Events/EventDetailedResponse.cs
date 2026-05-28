using LensmaniaLibrary.DTOs.Themes;
using LensmaniaLibrary.DTOs.Badges;

namespace LensmaniaLibrary.DTOs.Events;

public record EventDetailedResponse(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPremium,
    ThemeResponse Theme,
    List<BadgeResponse> Badges,
    string? CoverPhotoUrl = null
);
