using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Infrastructure.Persistence.Configurations;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(image => image.Id);
        builder.Property(image => image.Url).HasMaxLength(2048).IsRequired();
        builder.Property(image => image.AltText).HasMaxLength(200);
        builder.HasIndex(image => new { image.ProductId, image.SortOrder });
    }
}
