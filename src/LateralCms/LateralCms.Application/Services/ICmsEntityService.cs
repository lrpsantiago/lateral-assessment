using LateralCms.Application.Services.Contracts;

namespace LateralCms.Application.Services;

public interface ICmsEntityService
{
    /// <summary>
    /// Gets all versions of all entities.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<CmsEntityOutput>> GetAllEntitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the published version of all entities.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<CmsEntityOutput>> GetPublishedEntitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entityInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CmsEntityOutput> AddEntityAsync(CmsEntityInput entityInput, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the payload of an entity, creating a new version of it.
    /// </summary>
    /// <param name="payloadUpdateInput"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CmsEntityOutput> UpdateEntityAsync(EntityPayloadUpdateInput payloadUpdateInput, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard-delete an entity data.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteEntityAsync(string? entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a specific version of an entity. If the version is not given, it will publish the latest version.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="version"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishEntityAsync(string? entityId, int? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublish the published version of an entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UnpublishEntityAsync(string? entityId, CancellationToken cancellationToken = default);
}
