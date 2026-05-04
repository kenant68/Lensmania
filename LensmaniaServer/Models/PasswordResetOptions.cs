using System.ComponentModel.DataAnnotations;

namespace LensmaniaServer.Models;

public class PasswordResetOptions
{
    public int TokenLifetimeMinutes { get; set; } = 60;
    [Required]
    public string ClientBaseUrl { get; set; } = "";
}