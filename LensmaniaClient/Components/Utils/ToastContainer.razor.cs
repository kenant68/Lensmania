using LensmaniaClient.Services;
using LensmaniaLibrary.DTOs;
using LensmaniaLibrary.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LensmaniaClient.Components.Utils;

public partial class ToastContainer : IDisposable
{
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private readonly List<ToastItem> _toasts = [];
    private CancellationTokenSource? _cancellationTokenSource;

    protected override void OnInitialized()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        NotificationService.OnNotification += HandleNotification;
    }

    private void HandleNotification(Notification notification)
    {
        var toast = new ToastItem(notification);
        InvokeAsync(() =>
        {
            _toasts.Add(toast);
            StateHasChanged();
        });
        _ = AutoDismissToastAsync(toast, notification.DurationMs);
    }
    
    private async Task AutoDismissToastAsync(ToastItem toast, int durationMs)
    {
        try
        {
            await Task.Delay(durationMs, _cancellationTokenSource?.Token ?? default);

            await InvokeAsync(() =>
            {
                Remove(toast);
                StateHasChanged();
            });
        }
        catch (OperationCanceledException)
        {
            // Expected when component is disposed
        }
    }

    private void Remove(ToastItem toast) => _toasts.Remove(toast);

    public void Dispose()
    {
        NotificationService.OnNotification -= HandleNotification;
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private record ToastItem(Notification Notification)
    {
        public string Message => Notification.Message;
        
        public string CssClass => Notification.Type switch
        {
            NotificationType.Success => "success",
            NotificationType.Error   => "danger",
            NotificationType.Warning => "warning",
            NotificationType.Info    => "info",
            _ => "secondary"
        };
        
        public string Icon => Notification.Type switch
        {
            NotificationType.Success => "bi-check-circle-fill",
            NotificationType.Error   => "bi-x-circle-fill",
            NotificationType.Warning => "bi-exclamation-triangle-fill",
            NotificationType.Info    => "bi-info-circle-fill",
            _ => "bi-bell-fill"
        };
    }
}
