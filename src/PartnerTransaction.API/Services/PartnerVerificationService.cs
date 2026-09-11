using PartnerTransaction.API.Clients;
using PartnerTransaction.API.Constants;
using PartnerTransaction.API.Exceptions;
using Polly.Registry;

namespace PartnerTransaction.API.Services
{
    public interface PartnerVerificationService
    {
        Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default);
    }

    public class PartnerVerificationServiceImp(PartnerVerificationClient partnerVerificationClient,
        ResiliencePipelineProvider<string> pipelineProvider) : PartnerVerificationService
    {
        public async Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var pipeline = pipelineProvider.GetPipeline(PipelineConstants.PartnerVerificationPipeline);
                var response = await pipeline.ExecuteAsync(
                async token =>
                    await partnerVerificationClient.VerifyPartnerAsync(
                        partnerId,
                        token),
                cancellationToken);

                return response.Verified;
            }
            catch
            {
                throw new PartnerVerificationException("Partner verification failed.");
            }
        }
    }
}
