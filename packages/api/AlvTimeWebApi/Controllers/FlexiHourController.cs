using AlvTime.Business.FlexiHours;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class FlexiHourController : Controller
    {
        private readonly IFlexhourStorage _storage;
        private readonly RetrieveUsers _userRetriever;

        public FlexiHourController(RetrieveUsers userRetriever, IFlexhourStorage storage)
        {

            _storage = storage;
            _userRetriever = userRetriever;
        }

        [HttpGet("AvailableHours")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<AvailableHoursDto> FetchAvailableHours()
        {
            var user = _userRetriever.RetrieveUser();

            var availableHours = _storage.GetAvailableHours(user.Id, user.StartDate, DateTime.Now.Date);

            return Ok(new
            {
                AvailableHoursBeforeCompensation = availableHours.AvailableHoursBeforeCompensation,
                AvailableHoursAfterCompensation = availableHours.AvailableHoursAfterCompensation,
                Entries = availableHours.Entries.Select(entry => new
                {
                    Date = entry.Date.ToDateOnly(),
                    TaskId = entry.TaskId,
                    Hours = entry.Hours,
                    CompensationRate = entry.CompensationRate
                })
            });
        }

        [HttpGet("FlexedHours")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<FlexedHoursDto> FetchFlexedHours()
        {
            var user = _userRetriever.RetrieveUser();

            var flexedHours = _storage.GetFlexedHours(user.Id);

            return Ok(new
            {
                TotalHours = flexedHours.TotalHours,
                Entries = flexedHours.Entries.Select(entry => new
                {
                    Date = entry.Date.ToDateOnly(),
                    Hours = entry.Hours
                })
            });
        }

        [HttpGet("Payouts")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<PayoutsDto> FetchPaidOvertime()
        {
            var user = _userRetriever.RetrieveUser();

            var payouts = _storage.GetRegisteredPayouts(user.Id);

            return Ok(new
            {
                TotalHours = payouts.TotalHours,
                Entries = payouts.Entries.Select(entry => new
                {
                    Id = entry.Id,
                    Date = entry.Date.ToDateOnly(),
                    HoursBeforeCompRate = entry.HoursBeforeCompRate,
                    HoursAfterCompRate = entry.HoursAfterCompRate,
                    Active = entry.Active
                })
            });
        }

        [HttpPost("Payouts")]
        [Authorize]
        public ActionResult<GenericHourEntry> RegisterPaidOvertime([FromBody] HourEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values);
            }

            var user = _userRetriever.RetrieveUser();

            var response = _storage.RegisterPaidOvertime(new GenericHourEntry 
                    {
                        Hours = request.Hours,
                        Date = request.Date
                    }, user.Id);

            if (response != null)
            {
                return Ok(new
                {
                    Date = request.Date.ToDateOnly(),
                    Value = request.Hours
                });
            }

            return BadRequest("Not enough available hours");
        }

        [HttpDelete("Payouts")]
        [Authorize]
        public ActionResult<PaidOvertimeEntry> CancelPaidOvertime([FromQuery] int payoutId)
        {
            var user = _userRetriever.RetrieveUser();

            var response = _storage.CancelPayout(user.Id, payoutId);

            if (response.Id == 0)
            {
                return BadRequest($"No payout cancelled for payout id {payoutId} for user: {user.Email}. Has the payout been locked?");
            }

            return Ok(response);
        }
    }
}
