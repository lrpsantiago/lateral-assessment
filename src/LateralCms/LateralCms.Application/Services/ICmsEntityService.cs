using LateralCms.Application.Services.Contracts;

namespace LateralCms.Application.Services;

public interface ICmsEntityService
{
    Task<IEnumerable<CmsEntityOutput>> GetAllEntitiesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<CmsEntityOutput>> GetPublishedEntitiesAsync(CancellationToken cancellationToken = default);

    Task<CmsEntityOutput> AddEntityAsync(CmsEventParameters parameters, CancellationToken cancellationToken = default);

    Task<CmsEntityOutput> UpdateEntityAsync(CmsEventParameters parameters,
        CancellationToken cancellationToken = default);

    Task PublishEntityAsync(CmsEventParameters parameters, CancellationToken cancellationToken = default);

    Task UnpublishEntityAsync(CmsEventParameters parameters, CancellationToken cancellationToken = default);

    Task DeleteEntityAsync(string? entityId, CancellationToken cancellationToken = default);

    Task SetVisibilityAsync(string? entityId, bool isVisible, string? updatedBy = null,
        CancellationToken cancellationToken = default);
}
