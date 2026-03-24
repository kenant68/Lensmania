namespace LensmaniaServer.Models;

public class User {
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool IsAdmin { get; set; } = false;
    public bool IsPremium { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string Email, string Password);
}