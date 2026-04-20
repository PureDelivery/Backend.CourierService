using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Core.DTOs;

public class CourierAssignmentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CourierId { get; set; }
    public string PickupCode { get; set; } = string.Empty;
    public AssignmentStatus Status { get; set; }
    public DateTime AcceptedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
