using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;

namespace LateralCms.Application.Services;

public interface ICmsEventService
{
    Task<EventReceivalOutput> ReceiveAsync(IEnumerable<CmsEventInput> input, CancellationToken cancellationToken = default);
    Task ProcessAsync(CmsEvent cmsEvent, CancellationToken cancellationToken = default);
}
