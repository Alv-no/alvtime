
using System;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Payouts;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Responses;
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
        public PayoutsResponse FetchPaidOvertime()
        {
            var payouts = _payoutService.GetRegisteredPayouts();

            return new PayoutsResponse
            {
                TotalHoursBeforeCompRate = payouts.TotalHoursBeforeCompRate,
                TotalHoursAfterCompRate = payouts.TotalHoursAfterCompRate,
                Entries = payouts.Entries.Select(entry => new PayoutResponse()
                {
                    Id = entry.Id,
                    Date = entry.Date.ToDateOnly(),
                    HoursBeforeCompRate = entry.HoursBeforeCompRate,
                    HoursAfterCompRate = entry.HoursAfterCompRate,
                    Active = entry.Active
                }).ToList()
            };
        }
        
        [HttpPost("Payouts")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public GenericHourEntry RegisterPaidOvertime([FromBody] GenericHourEntry request)
        {
            var response = _payoutService.RegisterPayout(request);

            return new GenericHourEntry
            {
                Date = response.Date,
                Hours = response.HoursBeforeCompensation
            };
        }
        
        [HttpDelete("Payouts")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public PayoutResponse CancelPaidOvertime([FromQuery] int payoutId)
        {
            var response = _payoutService.CancelPayout(payoutId);

            return new PayoutResponse
            {
                Id = response.Id,
                Active = response.Date.Month >= DateTime.Now.Month && response.Date.Year == DateTime.Now.Year,
                Date = response.Date.ToDateOnly(),
                HoursAfterCompRate = response.HoursAfterCompensation,
                HoursBeforeCompRate = response.HoursBeforeCompensation
            };
        }
    }
}