using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Payouts;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Policy = "AdminPolicy")]
public class PayoutController(PayoutService payoutService) : ControllerBase
{
    [HttpGet("Payouts")]
    public async Task<ActionResult<List<PayoutAdminResponse>>> FetchPayoutsForAllUsers(DateTime? fromDate, DateTime? toDate)
    {
        var result = await payoutService.GetRegisteredPayoutsForAllUsers(fromDate, toDate);
        return result.Match<ActionResult<List<PayoutAdminResponse>>>(
            payouts => Ok(payouts.Entries.Select(entry => new PayoutAdminResponse
            {
                Id = entry.Id,
                UserId = entry.UserId,
                Date = entry.Date.ToDateOnly(),
                HoursBeforeCompRate = entry.HoursBeforeCompRate,
                HoursAfterCompRate = entry.HoursAfterCompRate,
                Active = entry.Active,
                CompensationRate = entry.CompRate
            }).ToList()),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent utbetalinger feilet med følgende feil")));
    }

    [HttpPut("LockPayouts")]
    public async Task<ActionResult> LockPaidOvertime([FromBody] DateTime lockDate)
    {
        var result = await payoutService.LockPayments(lockDate);
        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => BadRequest(errors.ToValidationProblemDetails("Lås utbetalinger feilet med følgende feil")));
    }
}