using System.ComponentModel.DataAnnotations;

namespace PureDelivery.CourierService.Core.DTOs;

public class RestaurantPickupRequest
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string PickupCode { get; set; } = string.Empty;
}
