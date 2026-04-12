using Microsoft.Extensions.Logging;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Core.Services.External;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Identity.Requests;

namespace PureDelivery.CourierService.Core.Services.impl
{
    public class CourierService : ICourierService
    {
        private readonly ICourierRepository _repository;
        private readonly IIdentityServiceClient _identityClient;
        private readonly ILogger<CourierService> _logger;

        public CourierService(
            ICourierRepository repository,
            IIdentityServiceClient identityClient,
            ILogger<CourierService> logger)
        {
            _repository = repository;
            _identityClient = identityClient;
            _logger = logger;
        }

        public async Task<BaseResponse<CourierDto>> CreateCourierAsync(
            CreateCourierRequest request, CancellationToken ct = default)
        {
            try
            {
                // 1. Проверяем уникальность email локально
                if (!await _repository.IsEmailUniqueAsync(request.Email, ct))
                    return BaseResponse<CourierDto>.Failure("Email already exists.");

                // 2. Создаём credentials в IdentityService
                var credentialResult = await _identityClient.RegisterUserCredentialAsync(
                    new RegisterUserCredentialRequest
                    {
                        Email = request.Email,
                        Password = request.Password,
                        Role = UserRole.Courier
                    }, ct);

                if (credentialResult == null)
                    return BaseResponse<CourierDto>.Failure(
                        "Failed to create user credentials in IdentityService.");

                // 3. Создаём профиль курьера локально
                var courier = new Courier
                {
                    UserId = credentialResult.UserId,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Phone = request.Phone.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    VehicleType = request.VehicleType
                };

                await _repository.AddAsync(courier, ct);

                _logger.LogInformation("Created courier {CourierId} (UserId: {UserId})",
                    courier.Id, courier.UserId);

                return BaseResponse<CourierDto>.Success(courier.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating courier {Email}", request.Email);
                return BaseResponse<CourierDto>.Failure($"Error creating courier: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourierDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                var courier = await _repository.GetByIdAsync(id, ct);
                if (courier == null)
                    return BaseResponse<CourierDto>.Failure("Courier not found.");

                return BaseResponse<CourierDto>.Success(courier.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courier {Id}", id);
                return BaseResponse<CourierDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<BaseResponse<CourierDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            try
            {
                var courier = await _repository.GetByUserIdAsync(userId, ct);
                if (courier == null)
                    return BaseResponse<CourierDto>.Failure("Courier not found.");

                return BaseResponse<CourierDto>.Success(courier.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courier by UserId {UserId}", userId);
                return BaseResponse<CourierDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeactivateAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                var result = await _repository.DeactivateAsync(id, ct);
                if (!result)
                    return BaseResponse<bool>.Failure("Courier not found.");

                return BaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating courier {Id}", id);
                return BaseResponse<bool>.Failure($"Error: {ex.Message}");
            }
        }
    }

    internal static class CourierExtensions
    {
        public static CourierDto ToDto(this Courier c) => new()
        {
            Id = c.Id,
            UserId = c.UserId,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Phone = c.Phone,
            Email = c.Email,
            VehicleType = c.VehicleType,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }
}
