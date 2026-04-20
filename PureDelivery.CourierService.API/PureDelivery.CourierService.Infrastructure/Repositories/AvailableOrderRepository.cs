using Microsoft.EntityFrameworkCore;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Infrastructure.Data;

namespace PureDelivery.CourierService.Infrastructure.Repositories;

public class AvailableOrderRepository(CourierDbContext context) : IAvailableOrderRepository
{
    public async Task<AvailableOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await context.AvailableOrders.FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

    public async Task<List<AvailableOrder>> GetAllAsync(CancellationToken ct = default) =>
        await context.AvailableOrders.OrderBy(o => o.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(AvailableOrder order, CancellationToken ct = default)
    {
        await context.AvailableOrders.AddAsync(order, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<AvailableOrder?> TakeAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await context.AvailableOrders.FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
        if (order == null) return null;

        context.AvailableOrders.Remove(order);
        await context.SaveChangesAsync(ct);
        return order;
    }
}
