using PureDelivery.CourierService.Core.Enums;

namespace PureDelivery.CourierService.Core.Models
{
    public class Courier
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// UserId из IdentityService — используется для аутентификации.
        /// </summary>
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public VehicleType VehicleType { get; set; } = VehicleType.Bicycle;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
