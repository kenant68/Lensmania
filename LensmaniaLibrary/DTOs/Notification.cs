using LensmaniaLibrary.Enums;

namespace LensmaniaLibrary.DTOs;

public record Notification(
    string Message, 
    NotificationType Type, 
    int DurationMs = 4000
);
