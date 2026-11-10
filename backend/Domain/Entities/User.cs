using Microsoft.AspNetCore.Identity;

namespace MDRN_HATS.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
