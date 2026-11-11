namespace MDRN_HATS.Domain.Entities;

public sealed class PickupPoint
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
