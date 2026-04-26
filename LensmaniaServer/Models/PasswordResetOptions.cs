namespace LensmaniaServer.Models;

public class PasswordResetOptions
{
    public int TokenLifetimeMinutes { get; set; } = 60;
    public string ClientBaseUrl { get; set; } = "";
}