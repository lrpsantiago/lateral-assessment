using LateralCms.Application.Interfaces.Queue;
using LateralCms.Application.Services;

namespace LateralCms.Api.BackgroundServices;

public sealed class CmsEventBatchWorker : BackgroundService
{
    private readonly ICmsEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CmsEventBatchWorker> _logger;

    public CmsEventBatchWorker(
        ICmsEventQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<CmsEventBatchWorker> logger)
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
                await using AsyncServiceScope scope =
                    _scopeFactory.CreateAsyncScope();

                ICmsEventProcessor processor = scope.ServiceProvider
                    .GetRequiredService<ICmsEventProcessor>();

                await processor.ProcessAsync(eventId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to process CMS event batch {BatchId}.",
                    eventId);
            }
        }
    }
}
