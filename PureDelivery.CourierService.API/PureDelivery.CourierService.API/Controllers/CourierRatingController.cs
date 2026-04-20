using Microsoft.AspNetCore.Mvc;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Services;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.CourierService.Controllers
{
    [ApiController]
    [Route("api/v1/courier/[controller]")]
    [Produces("application/json")]
    public class CourierRatingController : ControllerBase
    {
        private readonly ICourierRatingService _ratingService;
        private readonly ILogger<CourierRatingController> _logger;

        public CourierRatingController(
            ICourierRatingService ratingService,
            ILogger<CourierRatingController> logger)
        {
            _ratingService = ratingService;
            _logger        = logger;
        }

        /// <summary>
        /// Оставить оценку курьеру (клиент после завершённой доставки).
        /// </summary>
        [HttpPost("{courierId:guid}")]
        [ProducesResponseType(typeof(BaseResponse<CourierRatingDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<CourierRatingDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<CourierRatingDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<CourierRatingDto>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BaseResponse<CourierRatingDto>>> SubmitRating(
            [FromRoute] Guid courierId,
            [FromBody] SubmitRatingRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<CourierRatingDto>.Failure("Invalid request data"));

            var result = await _ratingService.SubmitRatingAsync(courierId, request, ct);

            if (!result.IsSuccess)
            {
                if (result.Error?.Contains("already rated") == true) return Conflict(result);
                if (result.Error?.Contains("not found") == true)     return NotFound(result);
                return BadRequest(result);
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Получить список оценок курьера с его средним рейтингом.
        /// </summary>
        [HttpGet("{courierId:guid}")]
        [ProducesResponseType(typeof(BaseResponse<PagedRatingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PagedRatingsDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<PagedRatingsDto>>> GetRatings(
            [FromRoute] Guid courierId,
            [FromQuery] int page     = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct     = default)
        {
            var result = await _ratingService.GetRatingsAsync(courierId, page, pageSize, ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }
    }
}
