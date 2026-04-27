using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.CourierService.Core.Services;

public interface ICourierAssignmentService
{
    Task<BaseResponse<List<AvailableOrderDto>>> GetAvailableOrdersAsync(Guid userId, CancellationToken ct = default);
    Task<BaseResponse<CourierAssignmentDto?>> GetActiveAssignmentAsync(Guid userId, CancellationToken ct = default);
    // userId = X-Validated-User-Id from gateway; service resolves the courier profile internally
    Task<BaseResponse<CourierAssignmentDto>> AcceptOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<BaseResponse<CourierAssignmentDto>> MarkPickedUpAsync(Guid userId, string pickupCode, CancellationToken ct = default);
    /// <summary>Restaurant confirms that the courier has picked up — they verify the code shown in courier's app.</summary>
    Task<BaseResponse<CourierAssignmentDto>> ConfirmPickupByRestaurantAsync(Guid orderId, string pickupCode, CancellationToken ct = default);
    Task<BaseResponse<CourierAssignmentDto>> MarkDeliveredAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<BaseResponse<CourierAssignmentDto>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
