using LensmaniaLibrary.DTOs.Badges;

namespace LensmaniaLibrary.DTOs.Events;

public record CreateEventRequest(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsPremium,
    int ThemeId,
    int UserId,
    List<CreateBadgeRequest> Badges
);
