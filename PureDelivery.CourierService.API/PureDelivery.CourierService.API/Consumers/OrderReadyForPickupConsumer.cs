using MassTransit;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace PureDelivery.CourierService.API.Consumers;

/// <summary>
/// Listens for OrderStatusChangedEvent. When status = ReadyForPickup, looks up
/// the assigned courier and publishes CourierOrderReadyEvent so NotificationService
/// can send a SignalR push to that courier's personal group.
/// </summary>
public class OrderReadyForPickupConsumer(
    ICourierAssignmentRepository assignmentRepo,
    ICourierRepository courierRepo,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderReadyForPickupConsumer> logger) : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var e = context.Message;

        if (e.NewStatus != OrderStatus.ReadyForPickup)
            return;

        var ct = context.CancellationToken;

        try
        {
            if (!Guid.TryParse(e.OrderId, out var orderId))
            {
                logger.LogWarning("OrderReadyForPickup: invalid OrderId '{OrderId}'", e.OrderId);
                return;
            }

            var assignment = await assignmentRepo.GetByOrderIdAsync(orderId, ct);
            if (assignment == null)
            {
                logger.LogWarning("OrderReadyForPickup: no assignment found for order {OrderId}", orderId);
                return;
            }

            var courier = await courierRepo.GetByIdAsync(assignment.CourierId, ct);
            if (courier == null)
            {
                logger.LogWarning("OrderReadyForPickup: courier {CourierId} not found", assignment.CourierId);
                return;
            }

            await publishEndpoint.Publish(new CourierOrderReadyEvent
            {
                OrderId       = e.OrderId,
                CourierUserId = courier.UserId.ToString(),
                RestaurantName = e.RestaurantName,
                ReadyAt        = e.ChangedAt
            }, ct);

            logger.LogInformation("Order {OrderId} ready — notifying courier {UserId}", orderId, courier.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ReadyForPickup for order {OrderId}", e.OrderId);
        }
    }
}
