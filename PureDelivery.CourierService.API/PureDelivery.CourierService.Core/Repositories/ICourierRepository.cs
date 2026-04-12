using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Core.Repositories
{
    public interface ICourierRepository
    {
        Task<Courier?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Courier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct = default);
        Task<Courier> AddAsync(Courier courier, CancellationToken ct = default);
        Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default);
    }
}
