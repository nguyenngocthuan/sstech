using Microsoft.Extensions.DependencyInjection;
using Moq;
using PartnerTransaction.API.Clients;
using PartnerTransaction.API.Constants;
using PartnerTransaction.API.Exceptions;
using PartnerTransaction.API.Models;
using PartnerTransaction.API.Services;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace PartnerTransaction.API.Tests.Resilience
{
    public class PartnerVerificationResilienceTests
    {
        private static (
        PartnerVerificationService Service,
        Mock<PartnerVerificationClient> Client)
        CreateService()
        {
            var client = new Mock<PartnerVerificationClient>();

            var services = new ServiceCollection();

            services.AddResiliencePipeline(
                PipelineConstants.PartnerVerificationPipeline,
                pipelineBuilder =>
                {
                    pipelineBuilder.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromMilliseconds(1),
                        BackoffType = DelayBackoffType.Constant,
                        ShouldHandle = new PredicateBuilder()
                            .Handle<TimeoutException>()
                    });
                });

            var serviceProvider = services.BuildServiceProvider();

            var pipelineProvider =
                serviceProvider.GetRequiredService<
                    ResiliencePipelineProvider<string>>();

            var service = new PartnerVerificationServiceImp(
                client.Object,
                pipelineProvider);

            return (service, client);
        }

        [Fact]
        public async Task VerifyPartnerAsync_WhenPartnerIsVerified_ShouldReturnTrue()
        {
            // Arrange
            var (service, client) = CreateService();

            client
                .Setup(x => x.VerifyPartnerAsync(
                    "P-1001",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartnerVerificationResponseModel
                {
                    PartnerId = "P-1001",
                    Verified = true
                });

            // Act
            var result = await service.VerifyPartnerAsync("P-1001");

            // Assert
            Assert.True(result);

            client.Verify(
                x => x.VerifyPartnerAsync(
                    "P-1001",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task VerifyAsync_WhenTimeoutOccurs_ShouldRetryAndSucceed()
        {
            // Arrange
            var (service, client) = CreateService();

            client
                .SetupSequence(x => x.VerifyPartnerAsync(
                    "P-1001",
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new TimeoutException(
                        "Partner Verification timed out."))
                .ThrowsAsync(
                    new TimeoutException(
                        "Partner Verification timed out."))
                .ReturnsAsync(
                    new PartnerVerificationResponseModel
                    {
                        PartnerId = "P-1001",
                        Verified = true
                    });

            // Act
            var result = await service.VerifyPartnerAsync("P-1001");

            // Assert
            Assert.True(result);

            // Initial attempt + 2 retries = 3 calls
            client.Verify(
                x => x.VerifyPartnerAsync(
                    "P-1001",
                    It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task VerifyAsync_WhenAllRetriesAreExhausted_ShouldThrowPartnerVerificationException()
        {
            // Arrange
            var (service, client) = CreateService();

            client
                .Setup(x => x.VerifyPartnerAsync(
                    "P-1001",
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new TimeoutException(
                        "Partner Verification timed out."));

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<PartnerVerificationException>(
                    () => service.VerifyPartnerAsync("P-1001"));

            Assert.Equal(
                "Partner verification failed.",
                exception.Message);

            // Initial attempt + 3 retries = 4 calls
            client.Verify(
                x => x.VerifyPartnerAsync(
                    "P-1001",
                    It.IsAny<CancellationToken>()),
                Times.Exactly(4));
        }
    }
}
