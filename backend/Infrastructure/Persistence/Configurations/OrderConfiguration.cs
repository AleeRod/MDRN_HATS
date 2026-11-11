
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(order => order.DeliveryMethod)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(order => order.CustomerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(order => order.CustomerEmail)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(order => order.CustomerPhone)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(order => order.DeliveryAddress)
            .HasMaxLength(500);

        builder.Property(order => order.DeliveryInstructions)
            .HasMaxLength(1000);

        builder.Property(order => order.PickupPointNameSnapshot)
            .HasMaxLength(150);

        builder.Property(order => order.PickupPointAddressSnapshot)
            .HasMaxLength(500);

        builder.Property(order => order.Total)
            .HasPrecision(12, 2);

        builder.HasIndex(order => new { order.UserId, order.CreatedAtUtc });

        builder.HasOne(order => order.User)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.PickupPoint)
            .WithMany(point => point.Orders)
            .HasForeignKey(order => order.PickupPointId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(order => order.Payments)
            .WithOne(payment => payment.Order)
            .HasForeignKey(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

