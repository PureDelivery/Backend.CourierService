using MassTransit;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Core.Services.External;
using PureDelivery.Shared.Contracts.DTOs.Location.Requests;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace PureDelivery.CourierService.API.Consumers;

public class OrderProcessedConsumer(
    IAvailableOrderRepository availableOrderRepo,
    ICourierRepository courierRepo,
    ILocationServiceClient locationClient,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderProcessedConsumer> logger) : IConsumer<OrderProcessedEvent>
{
    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
    {
        var e = context.Message;
        var ct = context.CancellationToken;

        try
        {
            var available = new AvailableOrder
            {
                OrderId = Guid.Parse(e.OrderId),
                OrderNumber = e.OrderNumber,
                RestaurantId = Guid.Parse(e.RestaurantId),
                RestaurantName = e.RestaurantName,
                DeliveryLatitude = e.DeliveryLatitude,
                DeliveryLongitude = e.DeliveryLongitude,
                DeliveryAddress = e.DeliveryAddress,
                DeliveryCity = e.DeliveryCity,
                RestaurantLatitude = e.RestaurantLatitude,
                RestaurantLongitude = e.RestaurantLongitude,
                RestaurantAddress = e.RestaurantAddress,
                RestaurantCity = e.RestaurantCity,
                DeliveryFee = e.DeliveryFee,
                TotalAmount = e.TotalAmount,
                CustomerName = e.CustomerName,
                CreatedAt = e.CreatedAt
            };

            await availableOrderRepo.AddAsync(available, ct);
            logger.LogInformation("Order {OrderId} saved to available pool", e.OrderId);

            // Get online/available couriers with known coordinates
            List<string> targetUserIds;
            try
            {
                var onlineCouriers = await courierRepo.GetOnlineAvailableAsync(
                    e.RestaurantLatitude, e.RestaurantLongitude,
                    maxRadiusKm: 25m, ct);

                if (onlineCouriers.Count == 0)
                {
                    targetUserIds = [];
                    logger.LogInformation("Order {OrderId}: no online couriers nearby", e.OrderId);
                }
                else
                {
                    var locationRequest = new CouriersInRangeRequest
                    {
                        RestaurantLatitude = e.RestaurantLatitude,
                        RestaurantLongitude = e.RestaurantLongitude,
                        DeliveryLatitude = e.DeliveryLatitude,
                        DeliveryLongitude = e.DeliveryLongitude,
                        Couriers = onlineCouriers.Select(c => new CourierLocationData
                        {
                            CourierUserId = c.UserId.ToString(),
                            Latitude = (decimal)(c.CurrentLatitude!.Value),
                            Longitude = (decimal)(c.CurrentLongitude!.Value),
                            VehicleType = c.VehicleType
                        }).ToList()
                    };

                    targetUserIds = await locationClient.GetCouriersInRangeAsync(locationRequest, ct);
                    logger.LogInformation("Order {OrderId}: {Count} couriers in range", e.OrderId, targetUserIds.Count);
                }
            }
            catch (Exception ex)
            {
                // LocationService down or courier lookup failed — broadcast to all online couriers
                logger.LogWarning(ex, "Order {OrderId}: courier range lookup failed, broadcasting to all", e.OrderId);
                targetUserIds = [];
            }

            await publishEndpoint.Publish(new OrderAvailableEvent
            {
                OrderId = available.OrderId,
                OrderNumber = available.OrderNumber,
                RestaurantId = available.RestaurantId,
                RestaurantName = available.RestaurantName,
                DeliveryLatitude = available.DeliveryLatitude,
                DeliveryLongitude = available.DeliveryLongitude,
                DeliveryAddress = available.DeliveryAddress,
                DeliveryCity = available.DeliveryCity,
                RestaurantLatitude = available.RestaurantLatitude,
                RestaurantLongitude = available.RestaurantLongitude,
                RestaurantAddress = available.RestaurantAddress,
                RestaurantCity = available.RestaurantCity,
                DeliveryFee = available.DeliveryFee,
                TotalAmount = available.TotalAmount,
                CustomerName = available.CustomerName,
                CreatedAt = available.CreatedAt,
                TargetCourierUserIds = targetUserIds
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process OrderProcessedEvent for order {OrderId}", e.OrderId);
            throw; // rethrow so MassTransit moves to error queue — don't silently swallow DB failures
        }
    }
}
