using Microsoft.Extensions.Options;

namespace LensmaniaServer.Services;

public class EventClosingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventClosingBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public EventClosingBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<EventClosingOptions> options,
        ILogger<EventClosingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(options.Value.IntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        // Run once immediately on startup so events that ended while the app was
        // down get closed without waiting a full interval, then on each tick.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var closer = scope.ServiceProvider.GetRequiredService<EventClosureService>();
                var closed = await closer.CloseDueEventsAsync(DateTime.UtcNow);
                if (closed > 0)
                    _logger.LogInformation("Closed {Count} due event(s).", closed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while closing due events.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
