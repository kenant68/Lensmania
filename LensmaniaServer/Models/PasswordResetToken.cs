using System.ComponentModel.DataAnnotations;

namespace LensmaniaServer.Models;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    [Timestamp] public byte[]? RowVersion { get; set; }
}