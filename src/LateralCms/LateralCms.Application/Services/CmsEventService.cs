using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Application.Interfaces.Queue;
using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;
using LateralCms.Domain.Enumerations;
using MapsterMapper;

namespace LateralCms.Application.Services;

public sealed class CmsEventService : ICmsEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICmsEventQueue _queue;
    private readonly ICmsEntityService _entityService;
    private readonly IMapper _mapper;

    public CmsEventService(
        IUnitOfWork unitOfWork,
        ICmsEventQueue queue,
        ICmsEntityService entityService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _queue = queue;
        _entityService = entityService;
        _mapper = mapper;
    }

    public async Task<EventReceivalOutput> ReceiveAsync(IEnumerable<CmsEventInput> input, CancellationToken cancellationToken = default)
    {
        var events = input
            .Select(x => _mapper.Map<CmsEvent>(x))
            .ToList();

        var batchId = Guid.NewGuid();

        foreach (var e in events)
        {
            e.BatchId = batchId;
            e.ReceivedAt = DateTime.UtcNow;
            e.Status = EventStatus.Pending;
        }

        await _unitOfWork.CmsEvents.AddRangeAsync(events, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var e in events)
        {
            await _queue.EnqueueAsync(e.Id, cancellationToken);
        }

        var eventIdList = events.Select(x => x.Id).ToList();
        var output = new EventReceivalOutput
        {
            BatchId = batchId,
            EventsIds = eventIdList
        };

        return output;
    }

    public async Task ProcessAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cmsEvent);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.TryParse(cmsEvent.Type, ignoreCase: true, out EventType eventType))
        {
            throw new InvalidOperationException(
                $"CMS event '{cmsEvent.Id}' has an unsupported type '{cmsEvent.Type}'.");
        }

        switch (eventType)
        {
            case EventType.Add:
                await HandleAddEventAsync(cmsEvent, cancellationToken);
                break;

            case EventType.Update:
                await HandleUpdateEventAsync(cmsEvent, cancellationToken);
                break;

            case EventType.Delete:
                await HandleDeleteEventAsync(cmsEvent, cancellationToken);
                break;

            case EventType.Publish:
                await HandlePublishEventAsync(cmsEvent, cancellationToken);
                break;

            case EventType.Unpublish:
                await HandleUnpublishEventAsync(cmsEvent, cancellationToken);
                break;

            default:
                throw new InvalidOperationException(
                    $"CMS event '{cmsEvent.Id}' has an unsupported type '{cmsEvent.Type}'.");
        }
    }

    private async Task HandleAddEventAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default)
    {
        var input = _mapper.Map<CmsEntityInput>(cmsEvent);
        await _entityService.AddEntityAsync(input, cancellationToken);
    }

    private async Task HandleUpdateEventAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default)
    {
        var input = _mapper.Map<EntityPayloadUpdateInput>(cmsEvent);
        await _entityService.UpdateEntityAsync(input, cancellationToken);
    }

    private async Task HandleDeleteEventAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default)
    {
        await _entityService.DeleteEntityAsync(cmsEvent.EntityId, cancellationToken);
    }

    private async Task HandlePublishEventAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default)
    {
        await _entityService.PublishEntityAsync(cmsEvent.EntityId, cmsEvent.Version, cancellationToken);
    }

    private async Task HandleUnpublishEventAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default)
    {
        await _entityService.UnpublishEntityAsync(cmsEvent.EntityId, cancellationToken);
    }
}
