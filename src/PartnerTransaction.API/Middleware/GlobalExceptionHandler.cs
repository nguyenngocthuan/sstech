using Microsoft.AspNetCore.Diagnostics;
using PartnerTransaction.API.Exceptions;

namespace PartnerTransaction.API.Middleware
{
    public class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(
                exception,
                "Unhandled exception occurred.");

            var response = exception switch
            {
                PartnerVerificationException => new
                {
                    statusCode = StatusCodes.Status503ServiceUnavailable,
                    success = false,
                    error = new
                    {
                        code = "PARTNER_VERIFICATION_FAILED",
                        message = "Unable to verify partner at this time."
                    }
                },

                TimeoutException => new
                {
                    statusCode = StatusCodes.Status503ServiceUnavailable,
                    success = false,
                    error = new
                    {
                        code = "PARTNER_VERIFICATION_TIMEOUT",
                        message = "Partner verification is temporarily unavailable."
                    }
                },

                _ => new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    success = false,
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        message = "An unexpected error occurred."
                    }
                }
            };

            httpContext.Response.StatusCode = response.statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}
