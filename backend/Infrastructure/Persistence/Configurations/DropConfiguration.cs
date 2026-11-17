using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Infrastructure.Persistence.Configurations;

public sealed class DropConfiguration : IEntityTypeConfiguration<Drop>
{
    public void Configure(EntityTypeBuilder<Drop> builder)
    {
        builder.ToTable("Drops", table =>
        {
            table.HasCheckConstraint(
                "CK_Drops_Slug_Format",
                "\"Slug\" ~ '^[a-z0-9-]+$'");
            table.HasCheckConstraint(
                "CK_Drops_PrimaryColor_Format",
                "\"PrimaryColor\" IS NULL OR \"PrimaryColor\" ~ '^#[0-9A-Fa-f]{6}$'");
            table.HasCheckConstraint(
                "CK_Drops_SecondaryColor_Format",
                "\"SecondaryColor\" IS NULL OR \"SecondaryColor\" ~ '^#[0-9A-Fa-f]{6}$'");
            table.HasCheckConstraint(
                "CK_Drops_BackgroundColor_Format",
                "\"BackgroundColor\" IS NULL OR \"BackgroundColor\" ~ '^#[0-9A-Fa-f]{6}$'");
            table.HasCheckConstraint(
                "CK_Drops_TextColor_Format",
                "\"TextColor\" IS NULL OR \"TextColor\" ~ '^#[0-9A-Fa-f]{6}$'");
            table.HasCheckConstraint(
                "CK_Drops_ButtonColor_Format",
                "\"ButtonColor\" IS NULL OR \"ButtonColor\" ~ '^#[0-9A-Fa-f]{6}$'");
        });
        builder.HasKey(drop => drop.Id);

        builder.Property(drop => drop.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(drop => drop.Description)
            .HasMaxLength(2000);

        builder.Property(drop => drop.Slug)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(drop => drop.CollaboratorName)
            .HasMaxLength(120);

        builder.Property(drop => drop.BannerUrl)
            .HasMaxLength(2048);

        builder.Property(drop => drop.PrimaryColor).HasMaxLength(7);
        builder.Property(drop => drop.SecondaryColor).HasMaxLength(7);
        builder.Property(drop => drop.BackgroundColor).HasMaxLength(7);
        builder.Property(drop => drop.TextColor).HasMaxLength(7);
        builder.Property(drop => drop.ButtonColor).HasMaxLength(7);

        builder.HasIndex(drop => drop.Slug)
            .IsUnique();
        builder.HasIndex(drop => new { drop.Active, drop.StartsAtUtc, drop.EndsAtUtc });

        builder.HasMany(drop => drop.Products)
            .WithOne(product => product.Drop)
            .HasForeignKey(product => product.DropId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
