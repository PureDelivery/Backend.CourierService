using PureDelivery.Shared.Contracts.DTOs.Location.Requests;
using PureDelivery.Shared.Contracts.DTOs.Location.Responses;

namespace PureDelivery.CourierService.Core.Services.External;

public interface ILocationServiceClient
{
    Task<List<string>> GetCouriersInRangeAsync(CouriersInRangeRequest request, CancellationToken ct = default);
}
