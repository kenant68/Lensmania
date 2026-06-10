namespace LensmaniaLibrary.DTOs.Users;

public record UserBadgeResponse(
    int BadgeId,
    string Name,
    string? ImageUrl,
    string EventName,
    DateTime AwardedAt,
    string? WinningPhotoUrl
);
