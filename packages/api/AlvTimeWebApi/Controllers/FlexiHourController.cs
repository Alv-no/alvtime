using AlvTime.Business.FlexiHours;
using AlvTimeWebApi.Controllers.Utils;
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
        public ActionResult<GenericHourEntry> RegisterPaidOvertime([FromBody] GenericHourEntry request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values);
            }

            var user = _userRetriever.RetrieveUser();

            var response = _storage.RegisterPaidOvertime(request, user.Id);

            return Ok(new GenericHourEntry
            {
                Date = response.Date,
                Hours = response.HoursBeforeCompensation
            });
        }

        [HttpDelete("Payouts")]
        [Authorize]
        public ActionResult<PaidOvertimeEntry> CancelPaidOvertime([FromQuery] int payoutId)
        {
            var user = _userRetriever.RetrieveUser();

            var response = _storage.CancelPayout(user.Id, payoutId);

            return Ok(response);
        }
    }
}