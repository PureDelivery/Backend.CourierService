using Microsoft.Extensions.Logging;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.Common.Http.Models;
using PureDelivery.Common.Http.Services;
using PureDelivery.Shared.Contracts.Configuration;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Location.Requests;
using PureDelivery.Shared.Contracts.DTOs.Location.Responses;

namespace PureDelivery.CourierService.Core.Services.External.impl;

public class LocationServiceClient(
    IHttpApiClient httpApiClient,
    ICustomConfigurationProvider configProvider,
    ILogger<LocationServiceClient> logger) : ILocationServiceClient
{
    private const string CouriersInRangeEndpoint = "/api/v1/location/couriers-in-range";

    private readonly LocationServiceConfiguration _config = configProvider
        .GetConfigurationAsync<LocationServiceConfiguration>("LocationService")
        .GetAwaiter().GetResult();

    public async Task<List<string>> GetCouriersInRangeAsync(
        CouriersInRangeRequest request, CancellationToken ct = default)
    {
        try
        {
            var requestParams = HttpRequestParams
                .WithBody(_config.BaseUrl, CouriersInRangeEndpoint, request)
                .WithCancellation(ct);

            var response = await httpApiClient
                .PostAsync<BaseResponse<CouriersInRangeResponse>>(requestParams);

            if (!response.IsSuccess || response.Data?.IsSuccess != true)
            {
                logger.LogError("LocationService error: {Status} {Message}",
                    response.StatusCode, response.ErrorMessage);
                return [];
            }

            return response.Data.Data?.CourierUserIds ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling LocationService couriers-in-range");
            return [];
        }
    }
}
