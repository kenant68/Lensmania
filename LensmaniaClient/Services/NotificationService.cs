using LensmaniaLibrary.Enums;
using LensmaniaLibrary.DTOs;

namespace LensmaniaClient.Services;

public class NotificationService
{
    public event Action<Notification>? OnNotification;

    public void Success(string message, int durationMs = 4000) =>
        OnNotification?.Invoke(new Notification(message, NotificationType.Success, durationMs));

    public void Error(string message, int durationMs = 5000) =>
        OnNotification?.Invoke(new Notification(message, NotificationType.Error, durationMs));

    public void Warning(string message, int durationMs = 4000) =>
        OnNotification?.Invoke(new Notification(message, NotificationType.Warning, durationMs));

    public void Info(string message, int durationMs = 4000) =>
        OnNotification?.Invoke(new Notification(message, NotificationType.Info, durationMs));
}
