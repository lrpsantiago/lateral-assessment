using LateralCms.Application.Extensions;
using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;
using LateralCms.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LateralCms.Application.Services;

public class CmsEntityService : ICmsEntityService
{
    private readonly IUnitOfWork _unitOfWork;

    public CmsEntityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CmsEntityOutput>> GetAllEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.CmsEntities
            .Query()
            .AsNoTracking()
            .Include(x => x.Versions)
            .Include(x => x.VisibilityOverride)
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
                var output = CreateEntityOutput(e, v, e.VisibilityOverride);

                resultList.Add(output);
            }
        }

        return resultList;
    }

    public async Task<IEnumerable<CmsEntityOutput>> GetPublishedEntitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.CmsEntities
            .Query()
            .AsNoTracking()
            .Include(x => x.PublishedVersion)
            .Include(x => x.VisibilityOverride)
            .Where(x => x.PublishedVersionId != null
                && (x.VisibilityOverride == null || x.VisibilityOverride.IsVisible))
            .ToListAsync(cancellationToken);

        var resultList = new List<CmsEntityOutput>();

        foreach (var e in entities)
        {
            var output = CreateEntityOutput(e, e.PublishedVersion!, e.VisibilityOverride);

            resultList.Add(output);
        }

        return resultList;
    }

    public async Task<CmsEntityOutput> AddEntityAsync(CmsEventParameters parameters,
        CancellationToken cancellationToken = default)
    {
        parameters.Validate();
        ValidatePayload(parameters, "add");

        var exists = await _unitOfWork.CmsEntities
            .AnyAsync(x => x.Id == parameters.EntityId, cancellationToken);

        if (exists)
        {
            throw new DomainException("An entity with the same Id already exists. No new entity was created.");
        }

        if (parameters.Version != 1)
        {
            throw new DomainException("A new entity must be created with version 1.");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = new CmsEntity
        {
            Id = parameters.EntityId,
            CreatedAt = DateTime.UtcNow,
            LatestVersionId = null,
            PublishedVersionId = null,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWork.CmsEntities.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var version = new CmsEntityVersion
        {
            EntityId = parameters.EntityId,
            Version = 1,
            Payload = parameters.Payload,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.CmsEntityVersions.Add(version);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entity.LatestVersionId = 1;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return CreateEntityOutput(entity, version);
    }

    public async Task<CmsEntityOutput> UpdateEntityAsync(CmsEventParameters parameters,
        CancellationToken cancellationToken = default)
    {
        parameters.Validate();
        ValidatePayload(parameters, "update");

        var entity = await GetValidatedEntityWithInclusionsAsync(parameters.EntityId, cancellationToken);
        var expectedVersion = entity!.LatestVersionId + 1;

        if (parameters.Version != expectedVersion)
        {
            throw new DomainException($"Invalid update version. Expected version {expectedVersion}, but received version {parameters.Version}.");
        }

        var now = DateTime.UtcNow;

        var version = new CmsEntityVersion
        {
            EntityId = parameters.EntityId,
            Version = expectedVersion!.Value,
            Payload = parameters.Payload,
            CreatedAt = now
        };

        entity.UpdatedAt = now;
        entity.LatestVersionId = expectedVersion;

        _unitOfWork.CmsEntityVersions.Add(version);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateEntityOutput(entity, version, entity.VisibilityOverride);
    }

    public Task PublishEntityAsync(CmsEventParameters parameters, CancellationToken cancellationToken = default)
    {
        return ApplySnapshotAsync(parameters, isPublished: true, cancellationToken);
    }

    public Task UnpublishEntityAsync(CmsEventParameters parameters, CancellationToken cancellationToken = default)
    {
        return ApplySnapshotAsync(parameters, isPublished: false, cancellationToken);
    }

    public async Task DeleteEntityAsync(string? entityId, CancellationToken cancellationToken = default)
    {
        var entity = await GetValidatedEntityAsync(entityId, cancellationToken);

        _unitOfWork.CmsEntities.Remove(entity!);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetVisibilityAsync(string? entityId, bool isVisible, string? updatedBy = null,
        CancellationToken cancellationToken = default)
    {
        await ValidateEntityAsync(entityId, cancellationToken);

        var visibilityOverride = await _unitOfWork.CmsEntityVisibilityOverrides
            .FirstOrDefaultAsync(x => x.CmsEntityId == entityId, cancellationToken);

        if (visibilityOverride == null)
        {
            visibilityOverride = new CmsEntityVisibilityOverride()
            {
                CmsEntityId = entityId,
            };

            _unitOfWork.CmsEntityVisibilityOverrides.Add(visibilityOverride);
        }

        visibilityOverride.IsVisible = isVisible;
        visibilityOverride.UpdatedAt = DateTime.UtcNow;
        visibilityOverride.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #region Helpers

    private static CmsEntityOutput CreateEntityOutput(CmsEntity entity, CmsEntityVersion version,
        CmsEntityVisibilityOverride? visibilityOverride = null)
    {
        return new CmsEntityOutput
        {
            Id = entity.Id,
            Version = version.Version,
            Payload = DeserializePayload(version.Payload),
            EntityCreatedAt = entity.CreatedAt,
            VersionCreatedAt = version.CreatedAt,
            IsPublished = entity.PublishedVersionId == version.Version,
            HasVisibilityAllowed = visibilityOverride == null || visibilityOverride.IsVisible
        };
    }

    private static JsonElement? DeserializePayload(string? payload)
    {
        return !string.IsNullOrWhiteSpace(payload)
            ? JsonSerializer.Deserialize<JsonElement>(payload)
            : null;
    }

    private async Task ValidateEntityAsync(string? entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new DomainException("The 'entityId' is required.");
        }

        var exists = await _unitOfWork.CmsEntities.AnyAsync(x => x.Id == entityId, cancellationToken);

        if (!exists)
        {
            throw new DomainException($"No entity was found with the given Id: {entityId}");
        }
    }

    private async Task<CmsEntity?> GetValidatedEntityAsync(string? entityId,
        CancellationToken cancellationToken = default)
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

    private async Task<CmsEntity?> GetValidatedEntityWithInclusionsAsync(string? entityId,
        CancellationToken cancellationToken = default)

    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new DomainException("The 'entityId' is required.");
        }

        var entity = await _unitOfWork.CmsEntities
            .Query()
            .Include(x => x.Versions)
            .Include(x => x.VisibilityOverride)
            .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);

        if (entity == null)
        {
            throw new DomainException($"No entity was found with the given Id: {entityId}");
        }

        return entity;
    }

    private async Task ApplySnapshotAsync(CmsEventParameters parameters, bool isPublished,
        CancellationToken cancellationToken)
    {
        parameters.Validate();

        var eventType = isPublished
            ? "publish"
            : "unpublish";

        ValidatePayload(parameters, eventType);

        var entityId = parameters.EntityId!;
        var versionNumber = parameters.Version.GetValueOrDefault();
        var now = DateTime.UtcNow;

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await _unitOfWork.CmsEntities
            .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);

        if (entity == null)
        {
            entity = new CmsEntity
            {
                Id = entityId,
                CreatedAt = now,
                UpdatedAt = now,
                LatestVersionId = null,
                PublishedVersionId = null
            };

            _unitOfWork.CmsEntities.Add(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (isPublished
            && entity.LatestVersionId is int latestVersion
            && versionNumber < latestVersion)
        {
            throw new DomainException(
                $"Cannot {eventType} stale version {versionNumber}. " +
                $"The latest version of entity '{entityId}' is {latestVersion}.");
        }

        var entityVersion = await _unitOfWork.CmsEntityVersions
            .FirstOrDefaultAsync(
                x => x.EntityId == entityId && x.Version == versionNumber,
                cancellationToken);

        if (entityVersion == null)
        {
            entityVersion = new CmsEntityVersion
            {
                EntityId = entityId,
                Version = versionNumber,
                Payload = parameters.Payload,
                CreatedAt = now
            };

            _unitOfWork.CmsEntityVersions.Add(entityVersion);
        }
        else
        {
            if (!string.Equals(entityVersion.Payload, parameters.Payload, StringComparison.Ordinal))
            {
                throw new DomainException(
                    $"Version {versionNumber} of entity '{entityId}' already exists with a different payload.");
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entity.LatestVersionId = entity.LatestVersionId is int currentLatestVersion
            ? Math.Max(currentLatestVersion, versionNumber)
            : versionNumber;
        entity.PublishedVersionId = isPublished ? versionNumber : null;
        entity.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidatePayload(CmsEventParameters parameters, string eventType)
    {
        if (string.IsNullOrWhiteSpace(parameters.Payload))
        {
            throw new DomainException($"The 'Payload' is required for {eventType} events.");
        }
    }

    #endregion
}
