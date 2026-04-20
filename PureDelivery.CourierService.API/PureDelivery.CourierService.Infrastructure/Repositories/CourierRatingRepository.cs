using Microsoft.EntityFrameworkCore;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Infrastructure.Data;

namespace PureDelivery.CourierService.Infrastructure.Repositories
{
    public class CourierRatingRepository : ICourierRatingRepository
    {
        private readonly CourierDbContext _context;

        public CourierRatingRepository(CourierDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasRatedAsync(
            Guid courierId, Guid orderId, Guid customerId, CancellationToken ct = default) =>
            await _context.CourierRatings.AnyAsync(
                r => r.CourierId == courierId &&
                     r.OrderId == orderId &&
                     r.RatedByCustomerId == customerId, ct);

        public async Task<CourierRating> AddAsync(CourierRating rating, CancellationToken ct = default)
        {
            await _context.CourierRatings.AddAsync(rating, ct);
            await _context.SaveChangesAsync(ct);
            return rating;
        }

        public async Task<List<CourierRating>> GetByCourierIdAsync(
            Guid courierId, int page, int pageSize, CancellationToken ct = default) =>
            await _context.CourierRatings
                .Where(r => r.CourierId == courierId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task<int> GetTotalCountAsync(Guid courierId, CancellationToken ct = default) =>
            await _context.CourierRatings.CountAsync(r => r.CourierId == courierId, ct);
    }
}
