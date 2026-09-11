using FluentValidation.TestHelper;
using PartnerTransaction.API.Models;
using PartnerTransaction.API.Validators;

namespace PartnerTransaction.API.Tests.Validators
{
    public class PartnerTransactionRequestValidatorTests
    {
        private readonly PartnerTransactionRequestValidator validator = new();

        private static PartnerTransactionRequestModel CreateValidRequest()
        {
            return new PartnerTransactionRequestModel
            {
                PartnerId = "P-1001",
                TransactionReference = "TXN-99823",
                Amount = 250.00m,
                Currency = "USD",
                Timestamp = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task Validate_WhenRequestIsValid_ShouldPass()
        {
            // Arrange
            var request = CreateValidRequest();

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WhenPartnerIdIsEmpty_ShouldFail()
        {
            // Arrange
            var request = CreateValidRequest();
            request.PartnerId = string.Empty;

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PartnerId)
                .WithErrorMessage("PartnerId is required.");
        }

        [Fact]
        public async Task Validate_WhenTransactionReferenceIsEmpty_ShouldFail()
        {
            // Arrange
            var request = CreateValidRequest();
            request.TransactionReference = string.Empty;

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(
                x => x.TransactionReference)
                .WithErrorMessage(
                    "TransactionReference is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task Validate_WhenAmountIsNotGreaterThanZero_ShouldFail(
            decimal amount)
        {
            // Arrange
            var request = CreateValidRequest();
            request.Amount = amount;

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage(
                    "Amount must be greater than 0.");
        }

        [Fact]
        public async Task Validate_WhenCurrencyIsEmpty_ShouldFail()
        {
            // Arrange
            var request = CreateValidRequest();
            request.Currency = string.Empty;

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage("Currency is required.");
        }

        [Theory]
        [InlineData("AUD")]
        [InlineData("VND")]
        [InlineData("ABC")]
        public async Task Validate_WhenCurrencyIsNotSupported_ShouldFail(
            string currency)
        {
            // Arrange
            var request = CreateValidRequest();
            request.Currency = currency;

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage(
                    "Currency must be one of: USD, EUR, GBP, JPY.");
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("usd")]
        [InlineData("EUR")]
        [InlineData("eur")]
        [InlineData("GBP")]
        [InlineData("JPY")]
        public async Task Validate_WhenCurrencyIsSupported_ShouldPass(
            string currency)
        {
            // Arrange
            var request = CreateValidRequest();
            request.Currency = currency;

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Currency);
        }
    }
}
