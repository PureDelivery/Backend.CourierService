using PureDelivery.Shared.Contracts.Domain.Enums;

namespace PureDelivery.CourierService.Core.Models
{
    public class Courier
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>UserId из IdentityService — используется для аутентификации.</summary>
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone    { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;

        public VehicleType VehicleType { get; set; } = VehicleType.Bicycle;

        // ── Location / availability ───────────────────────────────────────────
        public double?   CurrentLatitude     { get; set; }
        public double?   CurrentLongitude    { get; set; }
        /// <summary>Курьер включил режим «онлайн» (готов принимать заказы).</summary>
        public bool      IsOnline            { get; set; } = false;
        /// <summary>Нет активной доставки — доступен для нового заказа.</summary>
        public bool      IsAvailable         { get; set; } = false;
        public DateTime? LastLocationUpdated { get; set; }

        // ── Rating ────────────────────────────────────────────────────────────
        public double RatingSum    { get; set; } = 0;
        public int    RatingCount  { get; set; } = 0;
        public double AverageRating => RatingCount == 0 ? 0 : Math.Round(RatingSum / RatingCount, 2);

        // ── Stats ─────────────────────────────────────────────────────────────
        public int TotalDeliveries { get; set; } = 0;

        public bool     IsActive  { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────────
        public virtual ICollection<CourierRating> Ratings { get; set; } = [];
    }
}
