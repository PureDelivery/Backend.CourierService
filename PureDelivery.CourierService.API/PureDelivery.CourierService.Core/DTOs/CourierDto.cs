using PureDelivery.Shared.Contracts.Domain.Enums;

namespace PureDelivery.CourierService.Core.DTOs
{
    public class CourierDto
    {
        public Guid   Id       { get; set; }
        public Guid   UserId   { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }

        // Location / availability
        public double?   CurrentLatitude     { get; set; }
        public double?   CurrentLongitude    { get; set; }
        public bool      IsOnline            { get; set; }
        public bool      IsAvailable         { get; set; }
        public DateTime? LastLocationUpdated { get; set; }

        // Rating
        public double AverageRating   { get; set; }
        public int    TotalDeliveries { get; set; }

        public bool     IsActive  { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
