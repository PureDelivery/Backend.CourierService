using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.CourierService.Core.Services
{
    public interface ICourierService
    {
        Task<BaseResponse<CourierDto>> CreateCourierAsync(CreateCourierRequest request, CancellationToken ct = default);
        Task<BaseResponse<CourierDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<BaseResponse<CourierDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<BaseResponse<bool>> DeactivateAsync(Guid id, CancellationToken ct = default);
    }
}
