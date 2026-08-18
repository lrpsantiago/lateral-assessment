using LateralCms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LateralCms.Infrastructure.Database;

public class LateralCmsContext(DbContextOptions<LateralCmsContext> options) : DbContext(options)
{
    public DbSet<CmsEntity> CmsEntities { get; set; }
    public DbSet<CmsEntityVersion> CmsEntityVersions { get; set; }
    public DbSet<CmsEntityVisibilityOverride> CmsEntityVisibilityOverrides { get; set; }
    public DbSet<CmsEvent> CmsEvents { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LateralCmsContext).Assembly);

    }
}