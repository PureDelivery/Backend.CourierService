using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.CourierService.Core.Services
{
    public interface ICourierRatingService
    {
        Task<BaseResponse<CourierRatingDto>> SubmitRatingAsync(Guid courierId, SubmitRatingRequest request, CancellationToken ct = default);
        Task<BaseResponse<PagedRatingsDto>> GetRatingsAsync(Guid courierId, int page, int pageSize, CancellationToken ct = default);
    }
}
