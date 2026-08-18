using LateralCms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LateralCms.Infrastructure.Database.Configurations;

public class CmsEntityVersionConfiguration : IEntityTypeConfiguration<CmsEntityVersion>
{
    public void Configure(EntityTypeBuilder<CmsEntityVersion> builder)
    {
        builder.ToTable("CmsEntityVersion");

        builder.HasKey(version => new { version.EntityId, version.Version });

        builder.Property(version => version.EntityId)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(version => version.Version)
            .IsRequired();

        builder.Property(version => version.Payload);

        builder.Property(version => version.CreatedAt)
            .IsRequired();
    }
}
