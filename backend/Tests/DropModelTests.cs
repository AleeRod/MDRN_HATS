using Microsoft.EntityFrameworkCore;
using MDRN_HATS.Application.Services;
using MDRN_HATS.Application.Validation;
using MDRN_HATS.Domain.Entities;
using MDRN_HATS.Infrastructure.Persistence;

namespace MDRN_HATS.Tests;

public sealed class DropModelTests
{
    [Fact]
    public void DropSlug_HasUniqueDatabaseIndex()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=tests;Username=tests;Password=tests")
            .Options;

        using var context = new AppDbContext(options);
        var slugIndex = context.Model.FindEntityType(typeof(Drop))!
            .GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Drop.Slug)]));

        Assert.True(slugIndex.IsUnique);
    }

    [Fact]
    public void DropValidator_RejectsColorWithInvalidHexFormat()
    {
        var drop = new Drop
        {
            Slug = "summer-drop",
            PrimaryColor = "#12XZ56"
        };

        var result = new DropValidator().Validate(drop);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Drop.PrimaryColor));
    }

    [Fact]
    public void CurrentDropSelector_ReturnsNullWhenThereAreNoCurrentDrops()
    {
        var now = new DateTime(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);
        var drops = new[]
        {
            new Drop { Active = false },
            new Drop { Active = true, StartsAtUtc = now.AddMinutes(1) },
            new Drop { Active = true, EndsAtUtc = now.AddMinutes(-1) }
        };

        Assert.Null(CurrentDropSelector.Select(drops, now));
    }

    [Fact]
    public void CurrentDropSelector_ReturnsTheOnlyCurrentDrop()
    {
        var now = new DateTime(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);
        var currentDrop = new Drop
        {
            Slug = "current",
            Active = true,
            StartsAtUtc = now.AddHours(-1),
            EndsAtUtc = now.AddHours(1)
        };

        var selectedDrop = CurrentDropSelector.Select([currentDrop], now);

        Assert.Same(currentDrop, selectedDrop);
    }

    [Fact]
    public void CurrentDropSelector_PrefersTheMostRecentlyStartedOverlappingDrop()
    {
        var now = new DateTime(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);
        var earlierDrop = new Drop
        {
            Slug = "earlier",
            Active = true,
            StartsAtUtc = now.AddHours(-3),
            EndsAtUtc = now.AddHours(2)
        };
        var latestDrop = new Drop
        {
            Slug = "latest",
            Active = true,
            StartsAtUtc = now.AddHours(-1),
            EndsAtUtc = now.AddHours(1)
        };

        var selectedDrop = CurrentDropSelector.Select([earlierDrop, latestDrop], now);

        Assert.Same(latestDrop, selectedDrop);
    }
}
