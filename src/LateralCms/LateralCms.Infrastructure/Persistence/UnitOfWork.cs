using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Domain.Entities;
using LateralCms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace LateralCms.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly LateralCmsContext _context;

    public IRepository<CmsEntity> CmsEntities { get; }
    public IRepository<CmsEntityVersion> CmsEntityVersions { get; }
    public IRepository<CmsEntityVisibilityOverride> CmsEntityVisibilityOverrides { get; }
    public IRepository<CmsEvent> CmsEvents { get; }
    public IRepository<User> Users { get; }
    public IRepository<UserRole> UserRoles { get; }

    public UnitOfWork(LateralCmsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;

        CmsEntities = new Repository<CmsEntity>(context);
        CmsEntityVersions = new Repository<CmsEntityVersion>(context);
        CmsEntityVisibilityOverrides = new Repository<CmsEntityVisibilityOverride>(context);
        CmsEvents = new Repository<CmsEvent>(context);
        Users = new Repository<User>(context);
        UserRoles = new Repository<UserRole>(context);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public void ClearTrackedChanges()
    {
        _context.ChangeTracker.Clear();
    }

    public IDbContextTransaction BeginTransaction()
    {
        return _context.Database.BeginTransaction();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.Database.BeginTransactionAsync(cancellationToken);
    }
}
