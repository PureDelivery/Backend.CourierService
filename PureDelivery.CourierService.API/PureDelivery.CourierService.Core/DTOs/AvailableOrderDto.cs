namespace PureDelivery.CourierService.Core.DTOs;

public class AvailableOrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;

    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;

    public double RestaurantLatitude { get; set; }
    public double RestaurantLongitude { get; set; }
    public string RestaurantAddress { get; set; } = string.Empty;
    public string RestaurantCity { get; set; } = string.Empty;

    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
