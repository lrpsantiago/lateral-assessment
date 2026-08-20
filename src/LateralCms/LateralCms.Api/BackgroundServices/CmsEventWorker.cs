using LateralCms.Application.Interfaces.Queue;
using LateralCms.Application.Services;

namespace LateralCms.Api.BackgroundServices;

public sealed class CmsEventWorker : BackgroundService
{
    private readonly ICmsEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CmsEventWorker> _logger;

    public CmsEventWorker(
        ICmsEventQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<CmsEventWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var eventId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<ICmsEventProcessor>();
                
                _logger.LogInformation($"Event {eventId} process started...");

                await processor.ProcessAsync(eventId, stoppingToken);

                _logger.LogInformation($"Event {eventId} SUCCEED!");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Event {eventId} FAILED:", eventId);
            }
        }
    }
}
