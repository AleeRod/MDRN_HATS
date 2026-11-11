using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Infrastructure.Persistence.Configurations;

public sealed class DropConfiguration : IEntityTypeConfiguration<Drop>
{
    public void Configure(EntityTypeBuilder<Drop> builder)
    {
        builder.ToTable("Drops");
        builder.HasKey(drop => drop.Id);

        builder.Property(drop => drop.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(drop => drop.Description)
            .HasMaxLength(2000);

        builder.Property(drop => drop.BannerUrl)
            .HasMaxLength(2048);

        builder.HasIndex(drop => drop.Active);
        builder.HasIndex(drop => drop.StartsAtUtc);

        builder.HasMany(drop => drop.Products)
            .WithOne(product => product.Drop)
            .HasForeignKey(product => product.DropId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
