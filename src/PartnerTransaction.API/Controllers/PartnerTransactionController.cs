using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnerTransaction.API.Models;
using PartnerTransaction.API.Services;

namespace PartnerTransaction.API.Controllers
{
    [Route("api/v1/partner/transactions")]
    [ApiController]
    [Authorize(Policy = "TransactionWrite")]
    public class PartnerTransactionController(IValidator<PartnerTransactionRequestModel> validator,
        PartnerTransactionService partnerTransactionService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartnerTransactionRequestModel transaction, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(transaction, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(x => new { field = x.PropertyName, message = x.ErrorMessage }) 
                });
            }
            await partnerTransactionService.ProcessAsync(transaction, cancellationToken);
            return Accepted(new { Success = true, Message = "Transaction processed successfully" });
        }
    }
}
