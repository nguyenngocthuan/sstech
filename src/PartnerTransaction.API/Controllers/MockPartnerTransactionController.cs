using Microsoft.AspNetCore.Mvc;
using PartnerTransaction.API.Models;

namespace PartnerTransaction.API.Controllers
{
    [Route("api/v1/mock/partner/transactions")]
    [ApiController]
    public class MockPartnerTransactionController : ControllerBase
    {
        [HttpGet("{partnerId}")]
        public IActionResult Get(string partnerId)
        {
            var random = Random.Shared.Next(100);
            if (random < 30)
            {
                throw new TimeoutException("Partner Verification timed out");
            }
            return Ok(new PartnerVerificationResponseModel
            {
                PartnerId = partnerId,
                Verified = true
            });
        }
    }
}
