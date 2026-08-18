using LateralCms.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace LateralCms.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    IRepository<CmsEntity> CmsEntities { get; }
    IRepository<CmsEntityVersion> CmsEntityVersions { get; }
    IRepository<CmsEntityVisibilityOverride> CmsEntityVisibilityOverrides { get; }
    IRepository<CmsEvent> CmsEvents { get; }
    IRepository<User> Users { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void ClearTrackedChanges();
    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
