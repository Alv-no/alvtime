using AlvTime.Business;
using AlvTime.Business.Helpers;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using AlvTime.Business.Holidays;

namespace AlvTimeWebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class HolidayController : Controller
    {
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
        private readonly RetrieveUsers _userRetriever;
        private readonly ITimeEntryStorage _timeEntryStorage;
        private readonly IRedDaysService _redDaysService;

        public HolidayController(
            RetrieveUsers userRetriever, 
            IOptionsMonitor<TimeEntryOptions> timeEntryOptions, 
            ITimeEntryStorage timeEntryStorage,
            IRedDaysService redDaysService)
        {
            _userRetriever = userRetriever;
            _timeEntryOptions = timeEntryOptions;
            _timeEntryStorage = timeEntryStorage;
            _redDaysService = redDaysService;
        }

        [HttpGet("Holidays")]
        public ActionResult<IEnumerable<string>> FetchRedDays(int year = 0, int fromYearInclusive = 0, int toYearInclusive = 0)
        {
            return Ok(_redDaysService.GetRedDays(year, fromYearInclusive, toYearInclusive));
        }

        [HttpGet("user/UsedVacation")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<VacationOverviewDto> FetchUsedVacationHours([FromQuery] int year)
        {
            var user = _userRetriever.RetrieveUser();

            var entries = _timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year, 01, 01),
                ToDateInclusive = new DateTime(year, 12, 31),
                UserId = user.Id,
                TaskId = _timeEntryOptions.CurrentValue.PaidHolidayTask
            });

            var vacationOverview = VacationExtension.CalculateVacationOverview(entries);

            return Ok(vacationOverview);
        }
    }
}
