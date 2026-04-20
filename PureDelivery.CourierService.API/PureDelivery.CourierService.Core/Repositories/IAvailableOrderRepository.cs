using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Core.Repositories;

public interface IAvailableOrderRepository
{
    Task<AvailableOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<List<AvailableOrder>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(AvailableOrder order, CancellationToken ct = default);
    // Returns the removed record (null if already taken)
    Task<AvailableOrder?> TakeAsync(Guid orderId, CancellationToken ct = default);
}
