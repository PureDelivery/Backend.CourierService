using Microsoft.AspNetCore.Mvc;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Services;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.CourierService.Controllers;

[ApiController]
[Route("api/v1/courier/assignments")]
[Produces("application/json")]
public class CourierAssignmentController(
    ICourierAssignmentService assignmentService,
    ILogger<CourierAssignmentController> logger) : ControllerBase
{
    /// <summary>
    /// Returns the courier's current active assignment (Accepted or PickedUp), or null if none.
    /// Used to sync assignment state on app load.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<BaseResponse<CourierAssignmentDto?>>> GetActiveAssignment(
        [FromHeader(Name = "X-Validated-User-Id")] string? userId,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var courierId))
            return Unauthorized(BaseResponse<CourierAssignmentDto?>.Failure("Missing or invalid user identity."));

        var result = await assignmentService.GetActiveAssignmentAsync(courierId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get all orders currently available for pickup.
    /// Called by the courier app on connect and when SignalR sends a new order.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(BaseResponse<List<AvailableOrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<BaseResponse<List<AvailableOrderDto>>>> GetAvailableOrders(
        [FromHeader(Name = "X-Validated-User-Id")] string? userId,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var courierId))
            return Unauthorized(BaseResponse<List<AvailableOrderDto>>.Failure("Missing or invalid user identity."));

        var result = await assignmentService.GetAvailableOrdersAsync(courierId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Courier accepts an order — removes it from the available pool,
    /// creates an assignment with a pickup code, marks courier as busy,
    /// and notifies all other couriers via SignalR (through CourierAssignedEvent).
    /// </summary>
    [HttpPost("accept/{orderId:guid}")]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse<CourierAssignmentDto>>> AcceptOrder(
        [FromRoute] Guid orderId,
        [FromHeader(Name = "X-Validated-User-Id")] string? userId,
        CancellationToken ct = default)
    {
        // courierId comes from the gateway header (session-validated)
        if (!Guid.TryParse(userId, out var courierId))
            return Unauthorized(BaseResponse<CourierAssignmentDto>.Failure("Missing or invalid user identity."));

        // userId from IdentityService, but we need CourierService courierId.
        // Caller should pass the courier's profile Id in the route or the service
        // should resolve it. For simplicity, caller passes courierId directly.
        var result = await assignmentService.AcceptOrderAsync(courierId, orderId, ct);

        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Courier enters the 6-digit pickup code shown by the restaurant to confirm pickup.
    /// </summary>
    [HttpPost("pickup")]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse<CourierAssignmentDto>>> MarkPickedUp(
        [FromBody] VerifyPickupCodeRequest request,
        [FromHeader(Name = "X-Validated-User-Id")] string? userId,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var courierId))
            return Unauthorized(BaseResponse<CourierAssignmentDto>.Failure("Missing or invalid user identity."));

        var result = await assignmentService.MarkPickedUpAsync(courierId, request.PickupCode, ct);

        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Restaurant confirms courier pickup by verifying the 6-digit code shown in courier's app.
    /// </summary>
    [HttpPost("restaurant-pickup")]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse<CourierAssignmentDto>>> ConfirmRestaurantPickup(
        [FromBody] RestaurantPickupRequest request,
        CancellationToken ct = default)
    {
        var result = await assignmentService.ConfirmPickupByRestaurantAsync(request.OrderId, request.PickupCode, ct);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Courier confirms the order has been delivered to the customer.
    /// </summary>
    [HttpPost("deliver/{orderId:guid}")]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CourierAssignmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse<CourierAssignmentDto>>> MarkDelivered(
        [FromRoute] Guid orderId,
        [FromHeader(Name = "X-Validated-User-Id")] string? userId,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var courierId))
            return Unauthorized(BaseResponse<CourierAssignmentDto>.Failure("Missing or invalid user identity."));

        var result = await assignmentService.MarkDeliveredAsync(courierId, orderId, ct);

        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
