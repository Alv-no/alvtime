using AlvTime.Business;
using AlvTime.Business.Helpers;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AlvTime.Business.AbsenseDays;
using Microsoft.Extensions.Logging;

namespace AlvTimeWebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class HolidayController : Controller
    {
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
        private RetrieveUsers _userRetriever;
        private readonly ITimeEntryStorage _timeEntryStorage;
        private readonly ILogger<HolidayController> logger;
        private readonly IAbsenseDaysService absenseDaysService;

        public HolidayController(RetrieveUsers userRetriever,
                IOptionsMonitor<TimeEntryOptions> timeEntryOptions,
                ITimeEntryStorage timeEntryStorage,
                ILogger<HolidayController> logger,
                IAbsenseDaysService absenseDaysService)
        {
            this.absenseDaysService = absenseDaysService;
            this.logger = logger;
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

        [HttpGet("user/UsedVacation")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<VacationOverviewDto> FetchUsedVacationHours([FromQuery] int? year)
        {
            if (!year.HasValue) 
            {
                year = DateTime.Now.Year;
            }
            var user = _userRetriever.RetrieveUser();

            var entries = _timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year.Value, 01, 01),
                ToDateInclusive = new DateTime(year.Value, 12, 31),
                UserId = user.Id,
                TaskId = _timeEntryOptions.CurrentValue.PaidHolidayTask
            });

            var vacationOverview = VacationExtension.CalculateVacationOverview(entries);

            return Ok(vacationOverview);
        }

        [HttpGet("/user/RemainingVacationDays")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<AbsenseDaysDto> FetchRemainingAbsenseDays(int ?year, DateTime? intervalStart)
        {

            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }
            try
            {
                AbsenseDaysDto absenseDays = absenseDaysService.GetAbsenseDays(_userRetriever.RetrieveUser().Id, year.Value, intervalStart);
                return Ok(absenseDays);
            }
            catch (Exception e)
            {
                logger.LogError("Could not resolve remaining vacationdays for user {userid} on year {year} with error {error}", _userRetriever.RetrieveUser().Id, year, e.Message);
                throw;
            }
        }
    }
}
