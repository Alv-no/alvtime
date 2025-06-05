using AlvTime.Business.Options;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Absence;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace AlvTimeWebApi.Controllers;

[Route("api")]
[ApiController]
public class HolidayController : Controller
{
    private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
    private readonly IUserContext _userContext;
    private readonly ITimeRegistrationStorage _timeRegistrationStorage;
    private readonly IRedDaysService _redDaysService;
    private readonly ILogger<HolidayController> _logger;
    private readonly IAbsenceDaysService _absenceDaysService;

    public HolidayController(
        IUserContext userContext, 
        IOptionsMonitor<TimeEntryOptions> timeEntryOptions, 
        ITimeRegistrationStorage timeRegistrationStorage,
        IRedDaysService redDaysService,
        ILogger<HolidayController> logger,
        IAbsenceDaysService absenceDaysService)
    {
        _userContext = userContext;
        _timeEntryOptions = timeEntryOptions;
        _timeRegistrationStorage = timeRegistrationStorage;
        _redDaysService = redDaysService;
        _absenceDaysService = absenceDaysService;
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
    [Authorize]
    public async Task<ActionResult<VacationOverviewDto>> FetchUsedVacationHours([FromQuery] int? year)
    {
        if (!year.HasValue)
        {
            year = DateTime.Now.Year;
        }

        var user = await _userContext.GetCurrentUser();

        var entries = await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
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
    [Authorize]
    public async Task<ActionResult<VacationDaysDTO>> FetchVacationOverview([FromQuery] int? year, int? month, int? day)
    {
        if (!year.HasValue)
        {
            year = DateTime.Now.Year;
        }

        try
        {
            return Ok(await _absenceDaysService.GetAllTimeVacationOverview(year.Value));
        }
        catch (Exception e)
        {
            _logger.LogError("Could not resolve remaining vacation days for user on year {year} with error {error}", year, e.Message);
            throw;
        }
    }

    [HttpGet("user/AbsenseOverview")]
    [Authorize]
    public async Task<ActionResult<AbsenceDaysDto>> FetchRemainingAbsenseDays(int? year, DateTime? intervalStart)
    {

        var user = await _userContext.GetCurrentUser();
        if (!year.HasValue)
        {
            year = DateTime.Now.Year;
        }
        try
        {
            var absenceDays = await _absenceDaysService.GetAbsenceDays(user.Id, year.Value, intervalStart);
            return Ok(absenceDays);
        }
        catch (Exception e)
        {
            _logger.LogError("Could not resolve absense-days for user {userid} on year {year} with error {error}", user.Id, year, e.Message);
            throw;
        }
    }
}