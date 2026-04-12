using PureDelivery.CourierService.Core.Enums;

namespace PureDelivery.CourierService.Core.DTOs
{
    public class CourierDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
