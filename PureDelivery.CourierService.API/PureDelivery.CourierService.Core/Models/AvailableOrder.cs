namespace PureDelivery.CourierService.Core.Models;

// Transient record: exists only while the order is waiting for a courier.
// Deleted when a courier accepts it.
public class AvailableOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;

    public decimal DeliveryLatitude { get; set; }
    public decimal DeliveryLongitude { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;

    public decimal RestaurantLatitude { get; set; }
    public decimal RestaurantLongitude { get; set; }
    public string RestaurantAddress { get; set; } = string.Empty;
    public string RestaurantCity { get; set; } = string.Empty;

    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
