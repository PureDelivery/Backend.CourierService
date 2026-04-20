using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.Common.Http.Models;
using PureDelivery.Common.Http.Services;
using PureDelivery.Shared.Contracts.Configuration;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Identity.Requests;
using PureDelivery.Shared.Contracts.DTOs.Identity.Responses;

namespace PureDelivery.CourierService.Core.Services.External.impl
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private const string CreateCredentialEndpoint = "/api/internal/v1/identity/UserCredential";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly IHttpApiClient _httpApiClient;
        private readonly IdentityServiceConfiguration _config;
        private readonly ILogger<IdentityServiceClient> _logger;

        public IdentityServiceClient(
            IHttpApiClient httpApiClient,
            ICustomConfigurationProvider configProvider,
            ILogger<IdentityServiceClient> logger)
        {
            _httpApiClient = httpApiClient;
            _logger = logger;
            _config = configProvider
                .GetConfigurationAsync<IdentityServiceConfiguration>("IdentityService")
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
                        CreateCredentialEndpoint,
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

                BaseResponse<RegisterUserCredentialResult>? body = response.Data;
                if (body == null && !string.IsNullOrEmpty(response.RawContent))
                {
                    body = JsonSerializer.Deserialize<BaseResponse<RegisterUserCredentialResult>>(
                        response.RawContent, _jsonOptions);
                }

                if (body == null || !body.IsSuccess || body.Data == null)
                {
                    _logger.LogError("IdentityService error {StatusCode}: {Error}",
                        response.StatusCode, body?.Error ?? response.ErrorMessage);
                    return null;
                }

                return body.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling IdentityService for credential creation");
                return null;
            }
        }
    }
}
