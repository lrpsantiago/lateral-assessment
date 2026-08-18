namespace LateralCms.Application.Interfaces.Queue;

public interface ICmsEventQueue
{
    ValueTask EnqueueAsync(Guid eventId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default);
}
