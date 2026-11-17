using MDRN_HATS.Domain.Entities;

namespace MDRN_HATS.Application.Services;

public static class CurrentDropSelector
{
    public static Drop? Select(IEnumerable<Drop> drops, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(drops);

        return drops
            .Where(drop =>
                drop.Active &&
                (drop.StartsAtUtc is null || drop.StartsAtUtc <= utcNow) &&
                (drop.EndsAtUtc is null || drop.EndsAtUtc >= utcNow))
            .OrderByDescending(drop => drop.StartsAtUtc ?? DateTime.MinValue)
            .FirstOrDefault();
    }
}
