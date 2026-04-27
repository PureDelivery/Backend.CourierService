using MassTransit;
using Microsoft.Extensions.Logging;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.Events.Reviews;

namespace PureDelivery.CourierService.Core.Services.impl
{
    public class CourierRatingService : ICourierRatingService
    {
        private readonly ICourierRatingRepository _ratingRepo;
        private readonly ICourierRepository _courierRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CourierRatingService> _logger;

        public CourierRatingService(
            ICourierRatingRepository ratingRepo,
            ICourierRepository courierRepo,
            IPublishEndpoint publishEndpoint,
            ILogger<CourierRatingService> logger)
        {
            _ratingRepo = ratingRepo;
            _courierRepo = courierRepo;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<BaseResponse<CourierRatingDto>> SubmitRatingAsync(
            Guid courierId, SubmitRatingRequest request, CancellationToken ct = default)
        {
            try
            {
                var courier = await _courierRepo.GetByIdAsync(courierId, ct);
                if (courier == null)
                    return BaseResponse<CourierRatingDto>.Failure("Courier not found.");

                if (await _ratingRepo.HasRatedAsync(courierId, request.OrderId, request.RatedByCustomerId, ct))
                    return BaseResponse<CourierRatingDto>.Failure("You have already rated this courier for this order.");

                var rating = new CourierRating
                {
                    CourierId = courierId,
                    OrderId = request.OrderId,
                    RatedByCustomerId = request.RatedByCustomerId,
                    Score = request.Score,
                    Comment = request.Comment.Trim(),
                };

                await _ratingRepo.AddAsync(rating, ct);
                await _courierRepo.AddRatingAsync(courierId, request.Score, ct);

                _logger.LogInformation(
                    "Rating {Score} submitted for courier {CourierId} on order {OrderId}",
                    request.Score, courierId, request.OrderId);

                await _publishEndpoint.Publish(new CourierRatingSubmittedEvent
                {
                    RatingId          = rating.Id,
                    CourierId         = courierId,
                    RatedByCustomerId = request.RatedByCustomerId,
                    CustomerName      = request.CustomerName ?? string.Empty,
                    OrderId           = request.OrderId,
                    Score             = request.Score,
                    Comment           = request.Comment,
                    CreatedAt         = rating.CreatedAt,
                }, ct);

                return BaseResponse<CourierRatingDto>.Success(rating.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting rating for courier {CourierId}", courierId);
                return BaseResponse<CourierRatingDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PagedRatingsDto>> GetRatingsAsync(
            Guid courierId, int page, int pageSize, CancellationToken ct = default)
        {
            try
            {
                var courier = await _courierRepo.GetByIdAsync(courierId, ct);
                if (courier == null)
                    return BaseResponse<PagedRatingsDto>.Failure("Courier not found.");

                var ratings = await _ratingRepo.GetByCourierIdAsync(courierId, page, pageSize, ct);
                var total = await _ratingRepo.GetTotalCountAsync(courierId, ct);

                return BaseResponse<PagedRatingsDto>.Success(new PagedRatingsDto
                {
                    Items = ratings.Select(r => r.ToDto()).ToList(),
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize,
                    AverageScore = courier.AverageRating,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for courier {CourierId}", courierId);
                return BaseResponse<PagedRatingsDto>.Failure($"Error: {ex.Message}");
            }
        }
    }

    internal static class CourierRatingExtensions
    {
        public static CourierRatingDto ToDto(this CourierRating r) => new()
        {
            Id = r.Id,
            CourierId = r.CourierId,
            OrderId = r.OrderId,
            RatedByCustomerId = r.RatedByCustomerId,
            Score = r.Score,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
        };
    }
}
