using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Core.Repositories
{
    public interface ICourierRepository
    {
        Task<Courier?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Courier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<List<Courier>> GetOnlineAvailableAsync(double restaurantLat, double restaurantLng, double maxRadiusKm, CancellationToken ct = default);
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct = default);
        Task<Courier> AddAsync(Courier courier, CancellationToken ct = default);
        Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default);

        /// <summary>Обновляет координаты и статус онлайн/доступен.</summary>
        Task<bool> UpdateLocationAsync(Guid id, double lat, double lng, bool isOnline, bool isAvailable, CancellationToken ct = default);

        /// <summary>Добавляет оценку к агрегату курьера (RatingSum + RatingCount).</summary>
        Task<bool> AddRatingAsync(Guid id, int score, CancellationToken ct = default);

        /// <summary>Увеличивает счётчик доставок и устанавливает isAvailable=true.</summary>
        Task<bool> CompleteDeliveryAsync(Guid id, CancellationToken ct = default);
    }
}
