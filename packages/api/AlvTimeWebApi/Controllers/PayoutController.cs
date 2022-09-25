
using System;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Payouts;
using AlvTime.Business.Utils;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class PayoutController : ControllerBase
    {
        private readonly PayoutService _payoutService;

        public PayoutController(PayoutService payoutService)
        {
            _payoutService = payoutService;
        }
        
        [HttpGet("Payouts")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
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
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public async Task<GenericHourEntry> RegisterPaidOvertime([FromBody] PayoutRequest request)
        {
            var response = await _payoutService.RegisterPayout(new GenericHourEntry{ Date = request.Date, Hours = request.Hours});

            return new GenericHourEntry
            {
                Date = response.Date,
                Hours = response.HoursBeforeCompensation
            };
        }
        
        [HttpDelete("Payouts")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public async Task<ActionResult> CancelPaidOvertime([FromQuery] DateTime payoutDate)
        {
            await _payoutService.CancelPayout(payoutDate);
            return NoContent();
        }
    }
}