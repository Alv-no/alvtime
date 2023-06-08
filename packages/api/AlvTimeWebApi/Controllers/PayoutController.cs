
using System;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Payouts;
using AlvTimeWebApi.Authorization;
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
        public async Task<PayoutsResponse> FetchPaidOvertime()
        {
            var payouts = await _payoutService.GetRegisteredPayouts();

            return new PayoutsResponse
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
            };
        }
        
        [HttpPost("Payouts")]
        public async Task<GenericPayoutHourEntry> RegisterPaidOvertime([FromBody] PayoutRequest request)
        {
            var response = await _payoutService.RegisterPayout(new GenericPayoutHourEntry{ Date = request.Date, Hours = request.Hours});

            return new GenericPayoutHourEntry
            {
                Date = response.Date,
                Hours = response.HoursBeforeCompensation
            };
        }
        
        [HttpDelete("Payouts")]
        public async Task<ActionResult> CancelPaidOvertime([FromQuery] DateTime payoutDate)
        {
            await _payoutService.CancelPayout(payoutDate);
            return NoContent();
        }
    }
}