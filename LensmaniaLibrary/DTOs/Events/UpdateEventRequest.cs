using LensmaniaLibrary.DTOs.Badges;

namespace LensmaniaLibrary.DTOs.Events;

public record UpdateEventRequest(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPremium,
    int ThemeId,
    List<CreateBadgeRequest> Badges
);
