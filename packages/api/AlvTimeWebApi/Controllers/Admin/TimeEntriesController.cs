using AlvTime.Business.TimeRegistration;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AlvTimeWebApi.Authentication;
using System.Linq;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeRegistrationStorage _storage;

    public TimeEntriesController(ITimeRegistrationStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("TimeEntries")]
    public async Task<ActionResult<IEnumerable<TimeEntryEmployeeResponseDto>>> GetTimeEntriesForEmployees([FromQuery] int[] employeeIds, [FromQuery] int[] taskIds, DateTime fromDate, DateTime toDate)
    {
        try
        {
            return Ok((await _storage.GetTimeEntriesForEmployees(new MultipleTimeEntriesQuerySearch
                {
                    EmployeeIds = employeeIds,
                    FromDateInclusive = fromDate,
                    ToDateInclusive = toDate,
                    TaskIds = taskIds
                }))
                .Select(timeEntry => new
                {
                    User = timeEntry.User,
                    EmployeeId = timeEntry.EmployeeId,
                    Date = timeEntry.Date.ToDateOnly(),
                    Value = timeEntry.Value,
                    TaskId = timeEntry.TaskId,
                    ProjectId = timeEntry.ProjectId,
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
}