using LateralCms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LateralCms.Infrastructure.Database.Configurations;

public class CmsEntityVisibilityOverrideConfiguration : IEntityTypeConfiguration<CmsEntityVisibilityOverride>
{
    public void Configure(EntityTypeBuilder<CmsEntityVisibilityOverride> builder)
    {
        builder.ToTable("CmsEntityVisibilityOverride");

        builder.HasKey(visibilityOverride => visibilityOverride.CmsEntityId);

        builder.Property(visibilityOverride => visibilityOverride.CmsEntityId)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(visibilityOverride => visibilityOverride.IsVisible)
            .IsRequired();

        builder.Property(visibilityOverride => visibilityOverride.UpdatedAt)
            .IsRequired();

        builder.Property(visibilityOverride => visibilityOverride.UpdatedBy)
            .HasMaxLength(150);

        builder.HasOne(visibilityOverride => visibilityOverride.CmsEntity)
            .WithOne(entity => entity.VisibilityOverride)
            .HasForeignKey<CmsEntityVisibilityOverride>(visibilityOverride => visibilityOverride.CmsEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
