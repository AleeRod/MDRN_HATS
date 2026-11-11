namespace MDRN_HATS.Domain.Entities;

public sealed class Drop
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? BannerUrl { get; set; }

    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
