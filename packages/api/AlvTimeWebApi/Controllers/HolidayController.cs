using AlvTime.Business;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class HolidayController : Controller
    {
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
        private RetrieveUsers _userRetriever;
        private readonly ITimeEntryStorage _timeEntryStorage;

        public HolidayController(RetrieveUsers userRetriever, IOptionsMonitor<TimeEntryOptions> timeEntryOptions, ITimeEntryStorage timeEntryStorage)
        {
            _userRetriever = userRetriever;
            _timeEntryOptions = timeEntryOptions;
            _timeEntryStorage = timeEntryStorage;
        }

        [HttpGet("Holidays")]
        [Authorize]
        public ActionResult<IEnumerable<string>> FetchRedDays([FromQuery] int year)
        {
            var redDays = new RedDays(year);
            var dates = new List<string>();

            foreach (var date in redDays.Dates)
            {
                dates.Add(date.ToDateOnly());
            }

            return Ok(dates);
        }

        [HttpGet("user/UsedVacationDays")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TimeEntriesResponseDto>> FetchUsedVacationDays([FromQuery] int year)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year, 01, 01),
                ToDateInclusive = new DateTime(year, 12, 31),
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