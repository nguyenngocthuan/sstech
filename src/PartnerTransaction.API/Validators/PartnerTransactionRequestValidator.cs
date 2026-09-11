using FluentValidation;
using PartnerTransaction.API.Models;

namespace PartnerTransaction.API.Validators
{
    public class PartnerTransactionRequestValidator : AbstractValidator<PartnerTransactionRequestModel>
    {
        private static readonly string[] SupportedCurrencies = { "USD", "EUR", "GBP", "JPY" };
        public PartnerTransactionRequestValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("PartnerId is required.");
            RuleFor(x => x.TransactionReference)
                .NotEmpty()
                .WithMessage("TransactionReference is required.");
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");
            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Must(BeSupportedCurrency)
                .WithMessage("Currency must be one of: USD, EUR, GBP, JPY.");
            RuleFor(x => x.Timestamp)
                .NotEmpty()
                .WithMessage("Timestamp is required.");
        }
        private static bool BeSupportedCurrency(string? currency)
        {
            return currency != null && SupportedCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase);
        }
    }
}
