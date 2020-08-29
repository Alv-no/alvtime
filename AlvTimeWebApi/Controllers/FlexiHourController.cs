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

        [HttpGet("FlexiHours")]
        [Authorize]
        public ActionResult<IEnumerable<FlexiHoursResponseDto>> FetchFlexiHour(DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage
                .GetFlexihours(fromDateInclusive, toDateInclusive, user.Id)
                .Select(f => new FlexiHoursResponseDto
                {
                    Date = f.Date.ToDateOnly(),
                    Value = f.Value
                }));
        }

        [HttpPost("Overtime")]
        [Authorize]
        public ActionResult<RegisterPaidOvertimeResponseDto> RegisterPaidOvertime([FromBody] RegisterPaidOvertimeDto request)
        {
            var user = _userRetriever.RetrieveUser();

            var response = _storage.RegisterPaidOvertime(request.Date, request.Value, user.Id);

            return Ok(new RegisterPaidOvertimeResponseDto
            {
                Date = response.Date.ToDateOnly(),
                Value = response.Value
            });
        }
    }
}