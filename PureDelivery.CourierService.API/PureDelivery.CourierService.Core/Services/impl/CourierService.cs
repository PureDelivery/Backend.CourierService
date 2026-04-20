using MassTransit;
using Microsoft.Extensions.Logging;
using PureDelivery.CourierService.Core.DTOs;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Core.Services.External;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Identity.Requests;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace PureDelivery.CourierService.Core.Services.impl
{
    public class CourierService : ICourierService
    {
        private readonly ICourierRepository _repository;
        private readonly ICourierAssignmentRepository _assignmentRepository;
        private readonly IIdentityServiceClient _identityClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CourierService> _logger;

        public CourierService(
            ICourierRepository repository,
            ICourierAssignmentRepository assignmentRepository,
            IIdentityServiceClient identityClient,
            IPublishEndpoint publishEndpoint,
            ILogger<CourierService> logger)
        {
            _repository = repository;
            _assignmentRepository = assignmentRepository;
            _identityClient = identityClient;
            _publishEndpoint = publishEndpoint;
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

        public async Task<BaseResponse<CourierDto>> UpdateLocationAsync(
            Guid id, UpdateLocationRequest request, CancellationToken ct = default)
        {
            try
            {
                // isAvailable = online AND no active delivery (caller passes isOnline)
                var updated = await _repository.UpdateLocationAsync(
                    id, request.Latitude, request.Longitude,
                    request.IsOnline, request.IsOnline, ct);

                if (!updated)
                    return BaseResponse<CourierDto>.Failure("Courier not found.");

                var courier = await _repository.GetByIdAsync(id, ct);

                // If courier is actively delivering, push live location to the customer
                var activeAssignment = await _assignmentRepository.GetActiveAssignmentByCourierIdAsync(id, ct);
                if (activeAssignment != null)
                {
                    await _publishEndpoint.Publish(new CourierLocationUpdatedEvent
                    {
                        OrderId   = activeAssignment.OrderId.ToString(),
                        Latitude  = request.Latitude,
                        Longitude = request.Longitude,
                        UpdatedAt = DateTime.UtcNow
                    }, ct);
                }

                return BaseResponse<CourierDto>.Success(courier!.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for courier {Id}", id);
                return BaseResponse<CourierDto>.Failure($"Error: {ex.Message}");
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
            CurrentLatitude = c.CurrentLatitude,
            CurrentLongitude = c.CurrentLongitude,
            IsOnline = c.IsOnline,
            IsAvailable = c.IsAvailable,
            LastLocationUpdated = c.LastLocationUpdated,
            AverageRating = c.AverageRating,
            TotalDeliveries = c.TotalDeliveries,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
        };
    }
}
