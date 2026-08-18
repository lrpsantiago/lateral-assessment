using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Domain.Entities;
using LateralCms.Domain.Enumerations;

namespace LateralCms.Application.Services;

public sealed class CmsEventProcessor : ICmsEventProcessor
{
    private const int MaximumErrorMessageLength = 2000;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICmsEventService _eventService;

    public CmsEventProcessor(
        IUnitOfWork unitOfWork,
        ICmsEventService eventService)
    {
        _unitOfWork = unitOfWork;
        _eventService = eventService;
    }

    public async Task ProcessAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var cmsEvent = await LoadEventAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException($"CMS event '{eventId}' was not found.");

        if (cmsEvent.Status != EventStatus.Pending)
        {
            return;
        }

        var processStart = DateTime.UtcNow;

        try
        {
            await ProcessPendingEventAsync(cmsEvent, processStart, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _unitOfWork.ClearTrackedChanges();
            throw;
        }
        catch (Exception exception)
        {
            _unitOfWork.ClearTrackedChanges();
            await MarkFailedAsync(eventId, processStart, exception);
            throw;
        }
    }

    private async Task ProcessPendingEventAsync(CmsEvent cmsEvent, DateTime processStart, CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            cmsEvent.Status = EventStatus.Processing;
            cmsEvent.ProcessStart = processStart;
            cmsEvent.ProcessEnd = null;
            cmsEvent.LastErrorMessage = null;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventService.ProcessAsync(cmsEvent, cancellationToken);

            cmsEvent.Status = EventStatus.Completed;
            cmsEvent.ProcessEnd = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private Task<CmsEvent?> LoadEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return _unitOfWork.CmsEvents.SingleOrDefaultAsync(
            cmsEvent => cmsEvent.Id == eventId,
            cancellationToken);
    }

    private async Task MarkFailedAsync(Guid eventId, DateTime processStart, Exception exception)
    {
        var cmsEvent = await LoadEventAsync(eventId, CancellationToken.None);

        if (cmsEvent is null)
        {
            return;
        }

        cmsEvent.Status = EventStatus.Failed;
        cmsEvent.ProcessStart = processStart;
        cmsEvent.ProcessEnd = DateTime.UtcNow;
        cmsEvent.LastErrorMessage = Truncate(exception.Message);

        await _unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static string Truncate(string message)
    {
        return message.Length <= MaximumErrorMessageLength
            ? message
            : message[..MaximumErrorMessageLength];
    }
}
