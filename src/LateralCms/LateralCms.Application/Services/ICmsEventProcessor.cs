namespace LateralCms.Application.Services;

public interface ICmsEventProcessor
{
    Task ProcessAsync(Guid eventId, CancellationToken cancellationToken = default);
}
