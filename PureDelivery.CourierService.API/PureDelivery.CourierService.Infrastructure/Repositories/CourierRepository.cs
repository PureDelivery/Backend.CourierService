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
            !await _context.Couriers.AnyAsync(
                c => c.Email.ToLower() == email.ToLower(), ct);

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
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
