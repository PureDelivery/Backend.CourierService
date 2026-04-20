namespace PureDelivery.CourierService.Core.Models;

public enum AssignmentStatus
{
    Accepted = 1,     // courier accepted, hasn't picked up yet
    PickedUp = 2,     // courier picked up from restaurant
    Delivered = 3,    // delivered to customer
    Cancelled = 4
}

public class CourierAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CourierId { get; set; }

    // 6-character code shown to restaurant to confirm pickup
    public string PickupCode { get; set; } = string.Empty;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Accepted;

    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public virtual Courier? Courier { get; set; }
}
