using AlvTime.Business.Options;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class TimeEntriesController : Controller
{
    private readonly ITimeRegistrationStorage _storage;
    private readonly TimeRegistrationService _timeRegistrationService;
    private IUserContext _userContext;
    private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;

    private readonly int _reportUser;

    public TimeEntriesController(IUserContext userContext, ITimeRegistrationStorage storage, TimeRegistrationService timeRegistrationService, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
    {
        _userContext = userContext;
        _storage = storage;
        _timeRegistrationService = timeRegistrationService;
        _timeEntryOptions = timeEntryOptions;
        _reportUser = _timeEntryOptions.CurrentValue.ReportUser;
    }

    [HttpGet("TimeEntries")]
    public async Task<ActionResult<IEnumerable<TimeEntryResponseDto>>> FetchTimeEntries(DateTime fromDateInclusive, DateTime toDateInclusive)
    {
        try
        {
            var user = await _userContext.GetCurrentUser();
            return Ok((await _storage.GetTimeEntries(new TimeEntryQuerySearch
                {
                    UserId = user.Id,
                    FromDateInclusive = fromDateInclusive,
                    ToDateInclusive = toDateInclusive
                }))
                .Select(timeEntry => new
                {
                    User = timeEntry.User,
                    UserEmail = timeEntry.UserEmail,
                    Id = timeEntry.Id,
                    Date = timeEntry.Date.ToDateOnly(),
                    Value = timeEntry.Value,
                    TaskId = timeEntry.TaskId,
                    Comment = timeEntry.Comment,
                    CommentedAt = timeEntry.CommentedAt,
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
    public async Task<ActionResult<List<TimeEntryResponseDto>>> UpsertTimeEntry([FromBody] List<CreateTimeEntryDto> requests)
    {
        var result = await _timeRegistrationService.UpsertTimeEntry(requests);
        return result.Match<ActionResult<List<TimeEntryResponseDto>>>(
            timeEntries => Ok(timeEntries.Select(timeEntry => new
            {
                User = timeEntry.User,
                UserEmail = timeEntry.UserEmail,
                Id = timeEntry.Id,
                Date = timeEntry.Date.ToDateOnly(),
                Value = timeEntry.Value,
                TaskId = timeEntry.TaskId,
                Comment = timeEntry.Comment,
                CommentedAt = timeEntry.CommentedAt
            })),
            errors => BadRequest(errors.ToValidationProblemDetails("Timeføring feilet. Eventuelle kommentarer har blitt oppdatert.")));
    }
        
    [HttpGet("FlexedHours")]
    public async Task<ActionResult<TimeEntriesResponse>> FetchFlexedHours()
    {
        var flexedEntries = await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
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
    public async Task<ActionResult<IEnumerable<TimeEntryResponseDto>>> FetchTimeEntriesReport(DateTime fromDateInclusive, DateTime toDateInclusive)
    {
        var user = _userContext.GetCurrentUser();

        if (user.Id == _reportUser)
        {
            var report = (await _storage.GetTimeEntriesReport(new TimeEntryQuerySearch
                {
                    FromDateInclusive = fromDateInclusive,
                    ToDateInclusive = toDateInclusive
                }))
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
    [Authorize(Roles = "Admin")]
    public async Task RetriggerTimeEntriesOnDay([FromQuery] DateTime date, [FromQuery] int userId)
    {
        await _timeRegistrationService.RetriggerDate(date, userId);
    }
}