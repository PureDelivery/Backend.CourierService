using Microsoft.EntityFrameworkCore;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Infrastructure.Data;

namespace PureDelivery.CourierService.Infrastructure.Repositories;

public class CourierAssignmentRepository(CourierDbContext context) : ICourierAssignmentRepository
{
    public async Task<CourierAssignment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await context.CourierAssignments.FirstOrDefaultAsync(a => a.OrderId == orderId, ct);

    public async Task<CourierAssignment?> GetActiveByPickupCodeAsync(string code, CancellationToken ct = default) =>
        await context.CourierAssignments.FirstOrDefaultAsync(
            a => a.PickupCode == code && a.Status == AssignmentStatus.Accepted, ct);

    public async Task<CourierAssignment?> GetActiveAssignmentByCourierIdAsync(Guid courierId, CancellationToken ct = default) =>
        await context.CourierAssignments.FirstOrDefaultAsync(
            a => a.CourierId == courierId &&
                 (a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.PickedUp), ct);

    public async Task<List<CourierAssignment>> GetByCourierIdAsync(Guid courierId, CancellationToken ct = default) =>
        await context.CourierAssignments
            .Where(a => a.CourierId == courierId)
            .OrderByDescending(a => a.AcceptedAt)
            .ToListAsync(ct);

    public async Task AddAsync(CourierAssignment assignment, CancellationToken ct = default)
    {
        await context.CourierAssignments.AddAsync(assignment, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid id, AssignmentStatus status, CancellationToken ct = default)
    {
        var assignment = await context.CourierAssignments.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Assignment {id} not found");

        assignment.Status = status;
        if (status == AssignmentStatus.PickedUp) assignment.PickedUpAt = DateTime.UtcNow;
        if (status == AssignmentStatus.Delivered) assignment.DeliveredAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }
}
