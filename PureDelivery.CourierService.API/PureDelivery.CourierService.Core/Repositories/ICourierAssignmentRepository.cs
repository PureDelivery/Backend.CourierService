using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Core.Repositories;

public interface ICourierAssignmentRepository
{
    Task<CourierAssignment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<CourierAssignment?> GetActiveByPickupCodeAsync(string code, CancellationToken ct = default);
    /// <summary>Returns the in-progress assignment (Accepted or PickedUp) for this courier, or null.</summary>
    Task<CourierAssignment?> GetActiveAssignmentByCourierIdAsync(Guid courierId, CancellationToken ct = default);
    Task<List<CourierAssignment>> GetByCourierIdAsync(Guid courierId, CancellationToken ct = default);
    Task AddAsync(CourierAssignment assignment, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, AssignmentStatus status, CancellationToken ct = default);
}
