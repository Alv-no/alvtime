using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        [Authorize]
        public ActionResult<IEnumerable<AvailableHoursDto>> FetchFlexiHour()
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage
                .FetchAvailableHours(user.Id)
                .Select(f => new AvailableHoursDto
                {
                    Date = f.Date.ToDateOnly(),
                    Value = f.Value
                }));
        }

        [HttpGet("FlexedHours")]
        [Authorize]
        public ActionResult<decimal> FetchOvertimeEquivalents(DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage.GetOvertimeEquivalents(fromDateInclusive, toDateInclusive, user.Id));
        }

        [HttpGet("Payouts")]
        [Authorize]
        public ActionResult<IEnumerable<RegisterPaidOvertimeDto>> FetchPaidOvertime(DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            var user = _userRetriever.RetrieveUser();

            var response = _storage.GetRegisteredPayouts(fromDateInclusive, toDateInclusive, user.Id);

            return Ok(response.Select(r => new RegisterPaidOvertimeResponseDto
            {
                Date = r.Date.ToDateOnly(),
                Value = r.Value
            }));
        }

        [HttpPost("Payouts")]
        [Authorize]
        public ActionResult<RegisterPaidOvertimeResponseDto> RegisterPaidOvertime([FromBody] RegisterPaidOvertimeDto request)
        {
            var user = _userRetriever.RetrieveUser();

            var response = _storage.RegisterPaidOvertime(request, user.Id);

            return Ok(new RegisterPaidOvertimeResponseDto
            {
                Date = response.Date.ToDateOnly(),
                Value = response.Value
            });
        }
    }
}