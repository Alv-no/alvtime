using System;
using System.Threading.Tasks;
using AlvTime.Business.Payouts;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Policy = "AdminPolicy")]
public class PayoutController(PayoutService payoutService) : ControllerBase
{
    [HttpPut("LockPayouts")]
    public async Task<ActionResult> LockPaidOvertime([FromBody] DateTime lockDate)
    {
        var result = await payoutService.LockPayments(lockDate);
        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => BadRequest(errors.ToValidationProblemDetails("Lås utbetalinger feilet med følgende feil")));
    }
}