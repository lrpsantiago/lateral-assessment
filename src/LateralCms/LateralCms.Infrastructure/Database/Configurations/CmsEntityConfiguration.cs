using LateralCms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LateralCms.Infrastructure.Database.Configurations;

public class CmsEntityConfiguration : IEntityTypeConfiguration<CmsEntity>
{
    public void Configure(EntityTypeBuilder<CmsEntity> builder)
    {
        builder.ToTable("CmsEntity");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entity => entity.LatestVersionId);

        builder.Property(entity => entity.PublishedVersionId);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();

        builder.HasMany(entity => entity.Versions)
            .WithOne(version => version.Entity)
            .HasForeignKey(version => version.EntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.LatestVersion)
            .WithOne()
            .HasForeignKey<CmsEntity>(entity => new { entity.Id, entity.LatestVersionId })
            .HasPrincipalKey<CmsEntityVersion>(version => new { version.EntityId, version.Version })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.PublishedVersion)
            .WithOne()
            .HasForeignKey<CmsEntity>(entity => new { entity.Id, entity.PublishedVersionId })
            .HasPrincipalKey<CmsEntityVersion>(version => new { version.EntityId, version.Version })
            .OnDelete(DeleteBehavior.NoAction);

        //builder.HasMany(x => x.Events)
        //    .WithOne(e => e.Entity)
        //    .HasForeignKey(x => x.EntityId)
        //    .OnDelete(DeleteBehavior.NoAction);
    }
}
