using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
public class TimeEntriesController : Controller
{
    private readonly ITimeRegistrationStorage _storage;
    private readonly TimeRegistrationService _timeRegistrationService;
    private RetrieveUsers _userRetriever;
    private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;

    private readonly int _reportUser;

    public TimeEntriesController(RetrieveUsers userRetriever, ITimeRegistrationStorage storage, TimeRegistrationService timeRegistrationService, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
    {
        _userRetriever = userRetriever;
        _storage = storage;
        _timeRegistrationService = timeRegistrationService;
        _timeEntryOptions = timeEntryOptions;
        _reportUser = _timeEntryOptions.CurrentValue.ReportUser;
    }

    [HttpGet("TimeEntries")]
    [Authorize(Policy = "AllowPersonalAccessToken")]
    public ActionResult<IEnumerable<TimeEntryResponseDto>> FetchTimeEntries(DateTime fromDateInclusive, DateTime toDateInclusive)
    {
        try
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage.GetTimeEntries(new TimeEntryQuerySearch
                {
                    UserId = user.Id,
                    FromDateInclusive = fromDateInclusive,
                    ToDateInclusive = toDateInclusive
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
        catch (Exception e)
        {
            return BadRequest(new
            {
                Message = e.ToString(),
            });
        }
    }

    [HttpPost("TimeEntries")]
    [Authorize(Policy = "AllowPersonalAccessToken")]
    public ActionResult<List<TimeEntryResponseDto>> UpsertTimeEntry([FromBody] List<CreateTimeEntryDto> requests)
    {
        return Ok(_timeRegistrationService.UpsertTimeEntry(requests)
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
        
    [HttpGet("FlexedHours")]
    [Authorize(Policy = "AllowPersonalAccessToken")]
    public ActionResult<TimeEntriesResponse> FetchFlexedHours()
    {
        var flexedEntries = _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
        {
            TaskId = _timeEntryOptions.CurrentValue.FlexTask
        });

        return Ok(new TimeEntriesResponse
        {
            TotalHours = flexedEntries.Sum(entry => entry.Value),
            Entries = flexedEntries.Select(entry => new GenericTimeEntryResponse()
            {
                Date = entry.Date.ToDateOnly(),
                Hours = entry.Value
            }).ToList()
        });
    }

    [HttpGet("TimeEntriesReport")]
    [Authorize(Policy = "AllowPersonalAccessToken")]
    public ActionResult<IEnumerable<TimeEntryResponseDto>> FetchTimeEntriesReport(DateTime fromDateInclusive, DateTime toDateInclusive)
    {
        var user = _userRetriever.RetrieveUser();

        if (user.Id == _reportUser)
        {
            var report = _storage.GetTimeEntries(new TimeEntryQuerySearch
                {
                    FromDateInclusive = fromDateInclusive,
                    ToDateInclusive = toDateInclusive
                })
                .ToList()
                .Select(timeEntry => new
                {
                    User = timeEntry.User,
                    UserEmail = timeEntry.UserEmail,
                    Id = timeEntry.Id,
                    Date = timeEntry.Date.ToDateOnly(),
                    Value = timeEntry.Value,
                    TaskId = timeEntry.TaskId
                });

            return Ok(report);
        }

        return Unauthorized();
    }

    [HttpPut("retrigger")]
    [AuthorizeAdmin]
    public void RetriggerTimeEntriesOnDay([FromQuery] DateTime date, [FromQuery] int userId)
    {
        _timeRegistrationService.RetriggerDate(date, userId);
    }
}