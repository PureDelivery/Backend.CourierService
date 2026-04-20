using PureDelivery.Shared.Contracts.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PureDelivery.CourierService.Core.DTOs
{
    public class CreateCourierRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public VehicleType VehicleType { get; set; } = VehicleType.Bicycle;
    }
}
