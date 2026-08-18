using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;
using LateralCms.Domain.Exceptions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LateralCms.Application.Services;

public class CmsEntityService : ICmsEntityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CmsEntityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CmsEntityOutput>> GetAllEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.CmsEntities
            .Query()
            .AsNoTracking()
            .Include(x => x.Versions)
            .ToListAsync(cancellationToken);

        var resultList = new List<CmsEntityOutput>();

        foreach (var e in entities)
        {
            if (e.Versions == null)
            {
                continue;
            }

            foreach (var v in e.Versions)
            {
                var output = CreateEntityOutput(e, v);

                resultList.Add(output);
            }
        }

        return resultList;
    }

    public async Task<IEnumerable<CmsEntityOutput>> GetPublishedEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.CmsEntities
            .Query()
            .AsNoTracking()
            .Where(x => x.PublishedVersionId != null)
            .Include(x => x.PublishedVersion)
            .ToListAsync(cancellationToken);

        var resultList = new List<CmsEntityOutput>();

        foreach (var e in entities)
        {
            var output = CreateEntityOutput(e, e.PublishedVersion!);

            resultList.Add(output);
        }

        return resultList;
    }

    public async Task<CmsEntityOutput> AddEntityAsync(CmsEntityInput entityInput, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityInput.Id))
        {
            throw new DomainException($"The 'Id' is required.");
        }

        var exists = await _unitOfWork.CmsEntities
            .AnyAsync(x => x.Id == entityInput.Id, cancellationToken);

        if (exists)
        {
            throw new DomainException("An entity with the same Id already exists. No new entity was created.");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = new CmsEntity
        {
            Id = entityInput.Id,
            CreatedAt = DateTime.UtcNow,
            LatestVersionId = null,
            PublishedVersionId = null,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWork.CmsEntities.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var version = new CmsEntityVersion
        {
            EntityId = entityInput.Id,
            Version = 1,
            Payload = entityInput.Payload,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.CmsEntityVersions.Add(version);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entity.LatestVersionId = 1;
        entity.PublishedVersionId = 1;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return CreateEntityOutput(entity, version);
    }

    public async Task<CmsEntityOutput> UpdateEntityAsync(EntityPayloadUpdateInput payloadUpdateInput, CancellationToken cancellationToken = default)
    {
        var entity = await ValidateAndGetEntityWithVersionsAsync(payloadUpdateInput.EntityId, cancellationToken);

        var latestVersion = entity!.Versions!.Max(x => x.Version);
        var newlyCreatedVersion = latestVersion + 1;
        var now = DateTime.UtcNow;

        var version = new CmsEntityVersion
        {
            EntityId = payloadUpdateInput.EntityId,
            Version = newlyCreatedVersion,
            Payload = payloadUpdateInput.Payload,
            CreatedAt = now
        };

        entity.UpdatedAt = now;
        entity.LatestVersionId = newlyCreatedVersion;

        _unitOfWork.CmsEntityVersions.Add(version);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateEntityOutput(entity, version);
    }

    public async Task DeleteEntityAsync(string? entityId, CancellationToken cancellationToken = default)
    {
        var entity = await ValidateAndGetEntityAsync(entityId, cancellationToken);

        _unitOfWork.CmsEntities.Remove(entity!);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishEntityAsync(string? entityId, int? version = null, CancellationToken cancellationToken = default)
    {
        var entity = await ValidateAndGetEntityAsync(entityId, cancellationToken);

        var versionToPublish = version == null
            ? entity!.LatestVersionId
            : version;

        var versionExists = await _unitOfWork.CmsEntityVersions
            .AnyAsync(x => x.EntityId == entityId && x.Version == versionToPublish);

        if (!versionExists)
        {
            throw new DomainException($"Version {versionToPublish} does not exists for this entity. EntityId: {entityId}; Latest Version: {entity!.LatestVersionId}");
        }

        entity!.PublishedVersionId = versionToPublish;
        entity.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishEntityAsync(string? entityId, CancellationToken cancellationToken = default)
    {
        var entity = await ValidateAndGetEntityAsync(entityId, cancellationToken);

        entity!.PublishedVersionId = null;
        entity!.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #region Helpers

    private static CmsEntityOutput CreateEntityOutput(CmsEntity entity, CmsEntityVersion version)
    {
        return new CmsEntityOutput
        {
            Id = entity.Id,
            Version = version.Version,
            Payload = version.Payload,
            EntityCreatedAt = entity.CreatedAt,
            VersionCreatedAt = version.CreatedAt,
            IsPublished = entity.PublishedVersionId == version.Version,
        };
    }

    private async Task<CmsEntity?> ValidateAndGetEntityAsync(string? entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new DomainException("The 'entityId' is required.");
        }

        var entity = await _unitOfWork.CmsEntities
            .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);

        if (entity == null)
        {
            throw new DomainException($"No entity was found with the given Id: {entityId}");
        }

        return entity;
    }

    private async Task<CmsEntity?> ValidateAndGetEntityWithVersionsAsync(string? entityId, CancellationToken cancellationToken = default)

    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new DomainException("The 'entityId' is required.");
        }

        var entity = await _unitOfWork.CmsEntities
            .Query()
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);

        if (entity == null)
        {
            throw new DomainException($"No entity was found with the given Id: {entityId}");
        }

        return entity;
    }

    #endregion
}
