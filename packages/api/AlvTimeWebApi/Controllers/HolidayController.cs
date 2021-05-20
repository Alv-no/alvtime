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
using AlvTime.Business.AbsenseDays;
using AlvTime.Business.Holidays;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<HolidayController> _logger;
        private readonly IAbsenseDaysService _absenseDaysService;

        public HolidayController(
            RetrieveUsers userRetriever, 
            IOptionsMonitor<TimeEntryOptions> timeEntryOptions, 
            ITimeEntryStorage timeEntryStorage,
            IRedDaysService redDaysService,
            ILogger<HolidayController> logger,
            IAbsenseDaysService absenseDaysService)
        {
            _userRetriever = userRetriever;
            _timeEntryOptions = timeEntryOptions;
            _timeEntryStorage = timeEntryStorage;
            _redDaysService = redDaysService;
            _absenseDaysService = absenseDaysService;
            _logger = logger;
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

        [HttpGet("user/VacationOverview")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<VacationDaysDTO> FetchVacationOverview([FromQuery] int? year, int? month, int? day)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            if (!month.HasValue)
            {
                month = DateTime.Now.Month;
            }

            if (!day.HasValue)
            {
                day = DateTime.Now.Day;
            }

            var user = _userRetriever.RetrieveUser();

            try
            {
                return Ok(_absenseDaysService.GetVacationDays(user.Id, year.Value, month.Value, day.Value));
            }
            catch (Exception e)
            {
                _logger.LogError("Could not resolve remaining vacationdays for user {userid} on year {year} with error {error}", user.Id, year, e.Message);
                throw;
            }
        }


        [HttpGet("user/AbsenseOverview")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<AbsenseDaysDto> FetchRemainingAbsenseDays(int? year, DateTime? intervalStart)
        {

            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }
            try
            {
                AbsenseDaysDto absenseDays = _absenseDaysService.GetAbsenseDays(_userRetriever.RetrieveUser().Id, year.Value, intervalStart);
                return Ok(absenseDays);
            }
            catch (Exception e)
            {
                _logger.LogError("Could not resolve absense-days for user {userid} on year {year} with error {error}", _userRetriever.RetrieveUser().Id, year, e.Message);
                throw;
            }
        }
    }
}
