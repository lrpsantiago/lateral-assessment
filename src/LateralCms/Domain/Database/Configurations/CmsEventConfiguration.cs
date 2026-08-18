using LateralCms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LateralCms.Infrastructure.Database.Configurations;

public class CmsEventConfiguration : IEntityTypeConfiguration<CmsEvent>
{
    public void Configure(EntityTypeBuilder<CmsEvent> builder)
    {
        builder.ToTable("CmsEvent");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BatchId)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(150);

        builder.Property(x => x.Type)
            .HasMaxLength(50);

        builder.Property(x => x.Payload);

        builder.Property(x => x.Version);

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.Property(x => x.ProcessStart);

        builder.Property(x => x.ProcessEnd);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.LastErrorMessage)
            .HasMaxLength(2000);
    }
}
