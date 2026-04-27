using Microsoft.EntityFrameworkCore;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Infrastructure.Data;

namespace PureDelivery.CourierService.Infrastructure.Repositories
{
    public class CourierRepository : ICourierRepository
    {
        private readonly CourierDbContext _context;

        public CourierRepository(CourierDbContext context)
        {
            _context = context;
        }

        public async Task<Courier?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.Couriers.FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);

        public async Task<Courier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            await _context.Couriers.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct);


        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct = default) =>
            !await _context.Couriers.AnyAsync(c => c.Email.ToLower() == email.ToLower(), ct);

        public async Task<Courier> AddAsync(Courier courier, CancellationToken ct = default)
        {
            await _context.Couriers.AddAsync(courier, ct);
            await _context.SaveChangesAsync(ct);
            return courier;
        }

        public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default)
        {
            var courier = await _context.Couriers.FindAsync([id], ct);
            if (courier == null) return false;

            courier.IsActive = false;
            courier.IsOnline = false;
            courier.IsAvailable = false;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateLocationAsync(
            Guid id, double lat, double lng, bool isOnline, bool isAvailable,
            CancellationToken ct = default)
        {
            var courier = await _context.Couriers.FindAsync([id], ct);
            if (courier == null) return false;

            courier.CurrentLatitude = lat;
            courier.CurrentLongitude = lng;
            courier.IsOnline = isOnline;
            courier.IsAvailable = isAvailable;
            courier.LastLocationUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AddRatingAsync(Guid id, int score, CancellationToken ct = default)
        {
            var courier = await _context.Couriers.FindAsync([id], ct);
            if (courier == null) return false;

            courier.RatingSum += score;
            courier.RatingCount += 1;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CompleteDeliveryAsync(Guid id, CancellationToken ct = default)
        {
            var courier = await _context.Couriers.FindAsync([id], ct);
            if (courier == null) return false;

            courier.TotalDeliveries += 1;
            courier.IsAvailable = true;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<Courier>> GetOnlineAvailableAsync(double restaurantLat, double restaurantLng, double maxRadiusKm, CancellationToken ct = default)
        {
            double delta = maxRadiusKm / 111.0 * 1.2;

            return await _context.Couriers
                 .Where(c => c.IsActive && c.IsOnline && c.IsAvailable
                          && c.CurrentLatitude != null
                          && c.CurrentLongitude != null
                          && c.CurrentLatitude.Value >= restaurantLat - delta
                          && c.CurrentLatitude.Value <= restaurantLat + delta
                          && c.CurrentLongitude.Value >= restaurantLng - delta
                          && c.CurrentLongitude.Value <= restaurantLng + delta)
                 .ToListAsync(ct);
        }
    }
}
