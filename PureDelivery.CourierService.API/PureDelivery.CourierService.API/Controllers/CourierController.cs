using Microsoft.AspNetCore.Mvc;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Services;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.CourierService.Controllers
{
    [ApiController]
    [Route("api/v1/courier/[controller]")]
    [Produces("application/json")]
    public class CourierController : ControllerBase
    {
        private readonly ICourierService _courierService;
        private readonly ILogger<CourierController> _logger;

        public CourierController(
            ICourierService courierService,
            ILogger<CourierController> logger)
        {
            _courierService = courierService;
            _logger = logger;
        }

        /// <summary>
        /// Зарегистрировать нового курьера (учётные данные + профиль).
        /// Пароль хранится в IdentityService, профиль — здесь.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BaseResponse<CourierDto>>> Register(
            [FromBody] CreateCourierRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<CourierDto>.Failure("Invalid request data"));

            var result = await _courierService.CreateCourierAsync(request, ct);

            if (!result.IsSuccess)
            {
                if (result.Error?.Contains("already exists") == true)
                    return Conflict(result);
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Получить профиль курьера по его Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<CourierDto>>> GetById(
            [FromRoute] Guid id, CancellationToken ct = default)
        {
            var result = await _courierService.GetByIdAsync(id, ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Получить профиль курьера по UserId (возвращает IdentityService при логине).
        /// Используется после логина: AuthDto.customerId → GET /courier/by-user/{userId}
        /// </summary>
        [HttpGet("by-user/{userId:guid}")]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CourierDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<CourierDto>>> GetByUserId(
            [FromRoute] Guid userId, CancellationToken ct = default)
        {
            var result = await _courierService.GetByUserIdAsync(userId, ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Деактивировать курьера.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseResponse<bool>>> Deactivate(
            [FromRoute] Guid id, CancellationToken ct = default)
        {
            var result = await _courierService.DeactivateAsync(id, ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }
    }
}
