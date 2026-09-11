using PartnerTransaction.API.Clients;
using PartnerTransaction.API.Messaging;
using PartnerTransaction.API.Services;

namespace PartnerTransaction.API.DependencyInjections
{
    public static class ServiceRegistration
    {
        public static void AddPartnerTransactionServices(this IServiceCollection services)
        {
            services.AddScoped<PartnerTransactionService, PartnerTransactionServiceImp>();
            services.AddScoped<PartnerVerificationService, PartnerVerificationServiceImp>();
            services.AddScoped<TransactionMessageSender, TransactionMessageSenderImp>();
            services.AddScoped<PartnerVerificationClient, PartnerVerificationClientImp>();
            services.AddScoped<JwtTokenService, JwtTokenServiceImp>();
        }
    }
}
