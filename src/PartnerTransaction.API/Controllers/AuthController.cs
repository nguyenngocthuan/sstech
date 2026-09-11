using Microsoft.AspNetCore.Mvc;
using PartnerTransaction.API.Services;

namespace PartnerTransaction.API.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(JwtTokenService jwtTokenService) : ControllerBase
    {
        [HttpPost("token")]
        public IActionResult GenerateToken()
        {
            var token = jwtTokenService.GenerateToken(
                subject: "partner-client",
                scope: "transactions.write");

            return Ok(new
            {
                accessToken = token,
                tokenType = "Bearer",
                expiresIn = 3600
            });
        }
    }
}
