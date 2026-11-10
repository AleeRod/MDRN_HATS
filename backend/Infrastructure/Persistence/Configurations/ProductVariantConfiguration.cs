using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(variant => variant.Id);
        builder.Property(variant => variant.Color).HasMaxLength(80).IsRequired();
        builder.Property(variant => variant.Size).HasMaxLength(40).IsRequired();
        builder.Property(variant => variant.Sku).HasMaxLength(100).IsRequired();
        builder.HasIndex(variant => variant.Sku).IsUnique();
    }
}
