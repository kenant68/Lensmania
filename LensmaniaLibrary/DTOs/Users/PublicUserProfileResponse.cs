namespace LensmaniaLibrary.DTOs.Users;

public record PublicUserProfileResponse(
    int Id,
    string Username,
    IReadOnlyList<UserBadgeResponse> Badges
);
