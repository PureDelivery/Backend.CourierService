using Microsoft.Extensions.Logging;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.Common.Http.Models;
using PureDelivery.Common.Http.Services;
using PureDelivery.CourierService.Core.Configuration;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Identity.Requests;
using PureDelivery.Shared.Contracts.DTOs.Identity.Responses;

namespace PureDelivery.CourierService.Core.Services.External.impl
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly IHttpApiClient _httpApiClient;
        private readonly IdentityServiceConfig _config;
        private readonly ILogger<IdentityServiceClient> _logger;

        public IdentityServiceClient(
            IHttpApiClient httpApiClient,
            ICustomConfigurationProvider configProvider,
            ILogger<IdentityServiceClient> logger)
        {
            _httpApiClient = httpApiClient;
            _logger = logger;
            _config = configProvider
                .GetConfigurationAsync<IdentityServiceConfig>("IdentityService")
                .GetAwaiter()
                .GetResult();
        }

        public async Task<RegisterUserCredentialResult?> RegisterUserCredentialAsync(
            RegisterUserCredentialRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var requestParams = HttpRequestParams
                    .WithBody(
                        _config.BaseUrl,
                        _config.GetCreateCredentialEndpoint(),
                        request
                    )
                    .WithHeaders(new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" },
                        { "Accept", "application/json" }
                    })
                    .WithCancellation(ct);

                var response = await _httpApiClient
                    .PostAsync<BaseResponse<RegisterUserCredentialResult>>(requestParams);

                if (!response.IsSuccess)
                {
                    _logger.LogError("IdentityService HTTP error {StatusCode}: {Message}",
                        response.StatusCode, response.ErrorMessage);
                    return null;
                }

                if (response.Data?.IsSuccess != true || response.Data.Data == null)
                {
                    _logger.LogError("IdentityService business error: {Error}", response.Data?.Error);
                    return null;
                }

                return response.Data.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling IdentityService for credential creation");
                return null;
            }
        }
    }
}
