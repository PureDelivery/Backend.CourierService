using PureDelivery.Shared.Contracts.DTOs.Identity.Requests;
using PureDelivery.Shared.Contracts.DTOs.Identity.Responses;

namespace PureDelivery.CourierService.Core.Services.External
{
    public interface IIdentityServiceClient
    {
        Task<RegisterUserCredentialResult?> RegisterUserCredentialAsync(
            RegisterUserCredentialRequest request,
            CancellationToken ct = default);
    }
}
