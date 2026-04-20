using System.ComponentModel.DataAnnotations;

namespace PureDelivery.CourierService.Core.DTOs;

public class VerifyPickupCodeRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string PickupCode { get; set; } = string.Empty;
}
