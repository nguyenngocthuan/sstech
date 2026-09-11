using PartnerTransaction.API.Constants;
using PartnerTransaction.API.Models;
using System.Net;

namespace PartnerTransaction.API.Clients
{
    public interface PartnerVerificationClient
    {
        Task<PartnerVerificationResponseModel> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default);
    }

    public class PartnerVerificationClientImp(HttpClient httpClient, IConfiguration configuration) : PartnerVerificationClient
    {
        public async Task<PartnerVerificationResponseModel> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            var api = configuration["PartnerVerificationApi"] ?? ApiConstants.PartnerVerificationApi;
            var response = await httpClient.GetAsync($"{api}/{partnerId}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new TimeoutException(
                    "Partner Verification timed out");
            }
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PartnerVerificationResponseModel>(cancellationToken: cancellationToken);
            return result ?? new PartnerVerificationResponseModel
            {
                Verified = false,
                PartnerId = partnerId
            };
        }
    }
}
