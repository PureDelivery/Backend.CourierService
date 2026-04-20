using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Core.Repositories
{
    public interface ICourierRatingRepository
    {
        Task<bool> HasRatedAsync(Guid courierId, Guid orderId, Guid customerId, CancellationToken ct = default);
        Task<CourierRating> AddAsync(CourierRating rating, CancellationToken ct = default);
        Task<List<CourierRating>> GetByCourierIdAsync(Guid courierId, int page, int pageSize, CancellationToken ct = default);
        Task<int> GetTotalCountAsync(Guid courierId, CancellationToken ct = default);
    }
}
