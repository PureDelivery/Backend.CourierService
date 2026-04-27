using MassTransit;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.Shared.Contracts.Events.Reviews;

namespace PureDelivery.CourierService.API.Consumers;

public class CourierRatingReceivedConsumer(
    ICourierRepository courierRepo,
    ILogger<CourierRatingReceivedConsumer> logger) : IConsumer<CourierRatingSubmittedEvent>
{
    public async Task Consume(ConsumeContext<CourierRatingSubmittedEvent> context)
    {
        var e = context.Message;
        var updated = await courierRepo.AddRatingAsync(e.CourierId, e.Score, context.CancellationToken);
        if (updated)
            logger.LogInformation("Updated rating for courier {CourierId}: +{Score}", e.CourierId, e.Score);
        else
            logger.LogWarning("Courier {CourierId} not found when updating rating", e.CourierId);
    }
}
