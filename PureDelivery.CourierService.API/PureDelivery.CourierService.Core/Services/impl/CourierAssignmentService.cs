using MassTransit;
using Microsoft.Extensions.Logging;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Core.Services.External;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Location.Requests;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace PureDelivery.CourierService.Core.Services.impl;

public class CourierAssignmentService(
    IAvailableOrderRepository availableOrderRepo,
    ICourierAssignmentRepository assignmentRepo,
    ICourierRepository courierRepo,
    ILocationServiceClient locationServiceClient,
    IPublishEndpoint publishEndpoint,
    ILogger<CourierAssignmentService> logger) : ICourierAssignmentService
{
    public async Task<BaseResponse<CourierAssignmentDto?>> GetActiveAssignmentAsync(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var courier = await courierRepo.GetByUserIdAsync(userId, ct);
            if (courier == null)
                return BaseResponse<CourierAssignmentDto?>.Failure("Courier not found.");

            var assignment = await assignmentRepo.GetActiveAssignmentByCourierIdAsync(courier.Id, ct);
            return BaseResponse<CourierAssignmentDto?>.Success(assignment == null ? null : ToDto(assignment));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active assignment for user {UserId}", userId);
            return BaseResponse<CourierAssignmentDto?>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<BaseResponse<List<AvailableOrderDto>>> GetAvailableOrdersAsync(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var courier = await courierRepo.GetByUserIdAsync(userId, ct);
            if (courier == null)
                return BaseResponse<List<AvailableOrderDto>>.Failure("Courier not found.");

            if (courier.CurrentLatitude == null || courier.CurrentLongitude == null)
                return BaseResponse<List<AvailableOrderDto>>.Success([]);

            var allOrders = await availableOrderRepo.GetAllAsync(ct);
            if (allOrders.Count == 0)
                return BaseResponse<List<AvailableOrderDto>>.Success([]);

            var courierEntry = new CourierLocationData
            {
                CourierUserId = userId.ToString(),
                Latitude = (decimal)courier.CurrentLatitude.Value,
                Longitude = (decimal)courier.CurrentLongitude.Value,
                VehicleType = courier.VehicleType
            };

            var filtered = new List<AvailableOrderDto>();
            foreach (var order in allOrders)
            {
                var inRange = await locationServiceClient.GetCouriersInRangeAsync(
                    new CouriersInRangeRequest
                    {
                        RestaurantLatitude = order.RestaurantLatitude,
                        RestaurantLongitude = order.RestaurantLongitude,
                        DeliveryLatitude = order.DeliveryLatitude,
                        DeliveryLongitude = order.DeliveryLongitude,
                        Couriers = [courierEntry]
                    }, ct);

                if (inRange.Contains(userId.ToString()))
                    filtered.Add(ToDto(order));
            }

            return BaseResponse<List<AvailableOrderDto>>.Success(filtered);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting available orders for user {UserId}", userId);
            return BaseResponse<List<AvailableOrderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<BaseResponse<CourierAssignmentDto>> AcceptOrderAsync(
        Guid userId, Guid orderId, CancellationToken ct = default)
    {
        try
        {
            var courier = await courierRepo.GetByUserIdAsync(userId, ct);
            if (courier == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Courier not found.");
            var courierId = courier.Id;

            if (!courier.IsOnline || !courier.IsAvailable)
                return BaseResponse<CourierAssignmentDto>.Failure("Courier is not available.");

            // Atomically remove the order from the available pool (null = already taken)
            var available = await availableOrderRepo.TakeAsync(orderId, ct);
            if (available == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Order is no longer available.");

            var pickupCode = GeneratePickupCode();

            var assignment = new CourierAssignment
            {
                OrderId = orderId,
                OrderNumber = available.OrderNumber,
                CourierId = courierId,
                PickupCode = pickupCode,
                Status = AssignmentStatus.Accepted
            };

            await assignmentRepo.AddAsync(assignment, ct);

            // Mark courier as busy
            await courierRepo.UpdateLocationAsync(
                courierId,
                courier.CurrentLatitude ?? 0,
                courier.CurrentLongitude ?? 0,
                isOnline: true,
                isAvailable: false,
                ct);

            await publishEndpoint.Publish(new CourierAssignedEvent
            {
                OrderId = orderId.ToString(),
                CourierId = courierId.ToString(),
                CourierUserId = courier.UserId.ToString(),
                CourierFirstName = courier.FirstName,
                CourierLastName = courier.LastName,
                CourierPhone = courier.Phone,
                CourierLatitude = courier.CurrentLatitude,
                CourierLongitude = courier.CurrentLongitude,
                AssignedAt = assignment.AcceptedAt
            }, ct);

            logger.LogInformation("Courier {CourierId} accepted order {OrderId} with code {Code}",
                courierId, orderId, pickupCode);

            return BaseResponse<CourierAssignmentDto>.Success(ToDto(assignment));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accepting order {OrderId} by courier", orderId);
            return BaseResponse<CourierAssignmentDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<BaseResponse<CourierAssignmentDto>> MarkPickedUpAsync(
        Guid userId, string pickupCode, CancellationToken ct = default)
    {
        try
        {
            var courier = await courierRepo.GetByUserIdAsync(userId, ct);
            if (courier == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Courier not found.");

            var assignment = await assignmentRepo.GetActiveByPickupCodeAsync(pickupCode, ct);
            if (assignment == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Invalid or already used pickup code.");

            if (assignment.CourierId != courier.Id)
                return BaseResponse<CourierAssignmentDto>.Failure("This code belongs to a different courier.");

            await assignmentRepo.UpdateStatusAsync(assignment.Id, AssignmentStatus.PickedUp, ct);
            assignment.Status = AssignmentStatus.PickedUp;
            assignment.PickedUpAt = DateTime.UtcNow;

            await publishEndpoint.Publish(new OrderPickedUpEvent
            {
                OrderId   = assignment.OrderId.ToString(),
                CourierId = courier.Id.ToString(),
                PickedUpAt = assignment.PickedUpAt!.Value
            }, ct);

            await publishEndpoint.Publish(new OrderInDeliveryEvent
            {
                OrderId           = assignment.OrderId.ToString(),
                CourierUserId     = courier.UserId.ToString(),
                CourierFirstName  = courier.FirstName,
                CourierLastName   = courier.LastName,
                CourierPhone      = courier.Phone,
                CourierLatitude   = courier.CurrentLatitude,
                CourierLongitude  = courier.CurrentLongitude,
                PickedUpAt        = assignment.PickedUpAt!.Value
            }, ct);

            logger.LogInformation("Courier {CourierId} picked up order {OrderId}", courier.Id, assignment.OrderId);

            return BaseResponse<CourierAssignmentDto>.Success(ToDto(assignment));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking pickup for courier with UserId {UserId}", userId);
            return BaseResponse<CourierAssignmentDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<BaseResponse<CourierAssignmentDto>> ConfirmPickupByRestaurantAsync(
        Guid orderId, string pickupCode, CancellationToken ct = default)
    {
        try
        {
            var assignment = await assignmentRepo.GetByOrderIdAsync(orderId, ct);
            if (assignment == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Assignment not found for this order.");

            if (assignment.Status != AssignmentStatus.Accepted)
                return BaseResponse<CourierAssignmentDto>.Failure("Order has already been picked up or delivered.");

            if (assignment.PickupCode != pickupCode)
                return BaseResponse<CourierAssignmentDto>.Failure("Invalid pickup code.");

            var courier = await courierRepo.GetByIdAsync(assignment.CourierId, ct);
            if (courier == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Courier not found.");

            await assignmentRepo.UpdateStatusAsync(assignment.Id, AssignmentStatus.PickedUp, ct);
            assignment.Status = AssignmentStatus.PickedUp;
            assignment.PickedUpAt = DateTime.UtcNow;

            await publishEndpoint.Publish(new OrderPickedUpEvent
            {
                OrderId   = assignment.OrderId.ToString(),
                CourierId = courier.Id.ToString(),
                PickedUpAt = assignment.PickedUpAt!.Value
            }, ct);

            await publishEndpoint.Publish(new OrderInDeliveryEvent
            {
                OrderId              = assignment.OrderId.ToString(),
                CourierUserId        = courier.UserId.ToString(),
                CourierFirstName     = courier.FirstName,
                CourierLastName      = courier.LastName,
                CourierPhone         = courier.Phone,
                CourierLatitude      = courier.CurrentLatitude,
                CourierLongitude     = courier.CurrentLongitude,
                CourierVehicleType   = courier.VehicleType,
                PickedUpAt           = assignment.PickedUpAt!.Value
            }, ct);

            logger.LogInformation("Restaurant confirmed pickup for order {OrderId} by courier {CourierId}",
                orderId, courier.Id);

            return BaseResponse<CourierAssignmentDto>.Success(ToDto(assignment));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming restaurant pickup for order {OrderId}", orderId);
            return BaseResponse<CourierAssignmentDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<BaseResponse<CourierAssignmentDto>> MarkDeliveredAsync(
        Guid userId, Guid orderId, CancellationToken ct = default)
    {
        try
        {
            var courier = await courierRepo.GetByUserIdAsync(userId, ct);
            if (courier == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Courier not found.");

            var assignment = await assignmentRepo.GetByOrderIdAsync(orderId, ct);
            if (assignment == null)
                return BaseResponse<CourierAssignmentDto>.Failure("Assignment not found.");

            if (assignment.CourierId != courier.Id)
                return BaseResponse<CourierAssignmentDto>.Failure("Not your order.");

            if (assignment.Status != AssignmentStatus.PickedUp)
                return BaseResponse<CourierAssignmentDto>.Failure("Order has not been picked up yet.");

            await assignmentRepo.UpdateStatusAsync(assignment.Id, AssignmentStatus.Delivered, ct);
            assignment.Status = AssignmentStatus.Delivered;
            assignment.DeliveredAt = DateTime.UtcNow;

            await courierRepo.CompleteDeliveryAsync(courier.Id, ct);

            await publishEndpoint.Publish(new OrderDeliveredEvent
            {
                OrderId = orderId.ToString(),
                CourierId = courier.Id.ToString(),
                DeliveredAt = assignment.DeliveredAt!.Value
            }, ct);

            logger.LogInformation("Courier {CourierId} delivered order {OrderId}", courier.Id, orderId);

            return BaseResponse<CourierAssignmentDto>.Success(ToDto(assignment));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking delivery for courier with UserId {UserId} order {OrderId}", userId, orderId);
            return BaseResponse<CourierAssignmentDto>.Failure($"Error: {ex.Message}");
        }
    }

    // ────────────────────────────────────────────────────────────────

    private static string GeneratePickupCode() =>
        Random.Shared.Next(100_000, 999_999).ToString();

    private static AvailableOrderDto ToDto(AvailableOrder o) => new()
    {
        OrderId = o.OrderId,
        OrderNumber = o.OrderNumber,
        RestaurantId = o.RestaurantId,
        RestaurantName = o.RestaurantName,
        DeliveryLatitude = o.DeliveryLatitude,
        DeliveryLongitude = o.DeliveryLongitude,
        DeliveryAddress = o.DeliveryAddress,
        DeliveryCity = o.DeliveryCity,
        RestaurantLatitude = o.RestaurantLatitude,
        RestaurantLongitude = o.RestaurantLongitude,
        RestaurantAddress = o.RestaurantAddress,
        RestaurantCity = o.RestaurantCity,
        DeliveryFee = o.DeliveryFee,
        TotalAmount = o.TotalAmount,
        CustomerName = o.CustomerName,
        CreatedAt = o.CreatedAt
    };

    private static CourierAssignmentDto ToDto(CourierAssignment a) => new()
    {
        Id = a.Id,
        OrderId = a.OrderId,
        OrderNumber = a.OrderNumber,
        CourierId = a.CourierId,
        PickupCode = a.PickupCode,
        Status = a.Status,
        AcceptedAt = a.AcceptedAt,
        PickedUpAt = a.PickedUpAt,
        DeliveredAt = a.DeliveredAt
    };
}
