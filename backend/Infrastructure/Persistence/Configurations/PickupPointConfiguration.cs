using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Infrastructure.Persistence.Configurations;

public sealed class PickupPointConfiguration : IEntityTypeConfiguration<PickupPoint>
{
    public void Configure(EntityTypeBuilder<PickupPoint> builder)
    {
        builder.ToTable("PickupPoints");
        builder.HasKey(point => point.Id);

        builder.Property(point => point.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(point => point.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(point => point.Description)
            .HasMaxLength(1000);

        builder.HasIndex(point => point.Active);

        builder.HasMany(point => point.Orders)
            .WithOne(order => order.PickupPoint)
            .HasForeignKey(order => order.PickupPointId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
