
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business;
using AlvTime.Business.Payouts;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    [AuthorizePersonalAccessToken]
    public class PayoutController : ControllerBase
    {
        private readonly PayoutService _payoutService;

        public PayoutController(PayoutService payoutService)
        {
            _payoutService = payoutService;
        }
        
        [HttpGet("Payouts")]
        public async Task<ActionResult<PayoutsResponse>> FetchPaidOvertime()
        {
            var result = await _payoutService.GetRegisteredPayouts();
            return result.Match<ActionResult<PayoutsResponse>>(
                payouts => Ok(new PayoutsResponse
                {
                    TotalHoursBeforeCompRate = payouts.TotalHoursBeforeCompRate,
                    TotalHoursAfterCompRate = payouts.TotalHoursAfterCompRate,
                    Entries = payouts.Entries.Select(entry => new PayoutResponse
                    {
                        Id = entry.Id,
                        Date = entry.Date.ToDateOnly(),
                        HoursBeforeCompRate = entry.HoursBeforeCompRate,
                        HoursAfterCompRate = entry.HoursAfterCompRate,
                        Active = entry.Active,
                        CompensationRate = entry.CompRate
                    }).ToList()
                }),
                errors => BadRequest(errors.ToValidationProblemDetails("Hent utbetalinger feilet med følgende feil")));
        }
        
        [HttpPost("Payouts")]
        public async Task<ActionResult<GenericPayoutHourEntry>> RegisterPaidOvertime([FromBody] PayoutRequest request)
        {
            var result = await _payoutService.RegisterPayout(new GenericPayoutHourEntry{ Date = request.Date, Hours = request.Hours});
            return result.Match<ActionResult<GenericPayoutHourEntry>>(
                response => Ok(new GenericPayoutHourEntry
                {
                    Date = response.Date,
                    Hours = response.HoursBeforeCompensation
                }),
                errors => BadRequest(errors.ToValidationProblemDetails("Registrer utbetalinger feilet med følgende feil")));
        }
        
        [HttpDelete("Payouts")]
        public async Task<ActionResult> CancelPaidOvertime([FromQuery] DateTime payoutDate)
        {
            var result = await _payoutService.CancelPayout(payoutDate);
            return result.Match<ActionResult>(
                NoContent(),
                errors => BadRequest(errors.ToValidationProblemDetails("Avbestill utbetalinger feilet med følgende feil")));
        }

        [HttpPut("LockPayouts")]
        public async Task<ActionResult> LockPaidOvertime([FromBody] DateTime lockDate)
        {
            var result = await _payoutService.LockPayments(lockDate);
            return result.Match<ActionResult>(
                result => NoContent(),
                errors => BadRequest(errors.ToValidationProblemDetails("Lås utbetalinger feilet med følgende feil")));
        }
    }
}