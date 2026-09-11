using PartnerTransaction.API.Messaging;
using PartnerTransaction.API.Models;

namespace PartnerTransaction.API.Services
{
    public interface PartnerTransactionService
    {
        Task ProcessAsync(PartnerTransactionRequestModel requestModel, CancellationToken cancellationToken = default);
    }

    public class PartnerTransactionServiceImp(PartnerVerificationService partnerVerificationService,
        TransactionMessageSender transactionMessageSender) : PartnerTransactionService
    {
        public async Task ProcessAsync(PartnerTransactionRequestModel requestModel, CancellationToken cancellationToken = default)
        {
            var isVerified = await partnerVerificationService.VerifyPartnerAsync(requestModel.PartnerId ?? string.Empty, cancellationToken);
            if (!isVerified)
            {
                throw new InvalidOperationException($"Partner with ID {requestModel.PartnerId} is not verified.");
            }

            await transactionMessageSender.SendTransactionMessageAsync(requestModel, cancellationToken);
        }
    }
}
