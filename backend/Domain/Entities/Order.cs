
using MDRN_HATS.Domain.Enums;

namespace MDRN_HATS.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Usuario registrado (opcional)
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    // Comprador
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Entrega
    public DeliveryMethod DeliveryMethod { get; set; }
    public Guid? PickupPointId { get; set; }
    public PickupPoint? PickupPoint { get; set; }

    public string? DeliveryAddress { get; set; }
    public string? DeliveryInstructions { get; set; }

    // Datos históricos del punto de entrega
    public string? PickupPointNameSnapshot { get; set; }
    public string? PickupPointAddressSnapshot { get; set; }

    // Pedido
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

