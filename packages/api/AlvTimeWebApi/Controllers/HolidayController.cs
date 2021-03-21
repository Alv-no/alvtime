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

        [Obsolete("Use GET Holidays/Years")]
        [HttpGet("Holidays")]
        public ActionResult<IEnumerable<string>> FetchRedDays(int year)
        {
            return Ok(_redDaysService.GetRedDaysFromYear(year));
        }

        [HttpGet("Holidays/Years")]
        public ActionResult<IEnumerable<string>> FetchRedDaysFromPeriod(int fromYearInclusive, int toYearInclusive)
        {
            return Ok(_redDaysService.GetRedDaysFromYears(fromYearInclusive, toYearInclusive));
        }

        [HttpGet("user/UsedVacation")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TimeEntriesResponseDto>> FetchUsedVacationDays([FromQuery] int? year)
        {
            var user = _userRetriever.RetrieveUser();

            if (!year.HasValue) {
                year = DateTime.Now.Year;
            }

            return Ok(_timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year.Value, 01, 01),
                ToDateInclusive = new DateTime(year.Value, 12, 31),
                UserId = user.Id,
                TaskId = _timeEntryOptions.CurrentValue.PaidHolidayTask

            })
            .Select(timeEntry => new
            {
                User = timeEntry.User,
                UserEmail = timeEntry.UserEmail,
                Id = timeEntry.Id,
                Date = timeEntry.Date.ToDateOnly(),
                Value = timeEntry.Value,
                TaskId = timeEntry.TaskId
            }));
        }
    }
}
