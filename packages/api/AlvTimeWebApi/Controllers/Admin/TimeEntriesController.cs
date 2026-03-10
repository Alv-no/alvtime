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
[Authorize(Policy = "AdminPolicy")]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeRegistrationStorage _storage;

    public TimeEntriesController(ITimeRegistrationStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("TimeEntries")]
    public async Task<ActionResult<IEnumerable<TimeEntryEmployeeResponseDto>>> GetTimeEntriesForEmployees([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            return Ok(await _storage.GetTimeEntriesForEmployees(new MultipleTimeEntriesQuerySearch
            {
                FromDateInclusive = fromDate,
                ToDateInclusive = toDate,
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