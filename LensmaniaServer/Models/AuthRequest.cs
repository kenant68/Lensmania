using System.ComponentModel.DataAnnotations;

namespace LensmaniaServer.Models;

public record RegisterRequest(
    [property: Required, MinLength(3), MaxLength(32)] string Username,
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MinLength(8), MaxLength(128)] string Password
);

public record LoginRequest(
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MinLength(8), MaxLength(128)] string Password
);