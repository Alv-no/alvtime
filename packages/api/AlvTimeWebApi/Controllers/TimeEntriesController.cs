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
    [Route("api/user")]
    [ApiController]
    public class TimeEntriesController : Controller
    {
        private readonly ITimeEntryStorage _storage;
        private readonly TimeEntryCreator _creator;
        private RetrieveUsers _userRetriever;
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;

        private readonly int _reportUser;

        public TimeEntriesController(RetrieveUsers userRetriever, ITimeEntryStorage storage, TimeEntryCreator creator, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            _userRetriever = userRetriever;
            _storage = storage;
            _creator = creator;
            _timeEntryOptions = timeEntryOptions;
            _reportUser = _timeEntryOptions.CurrentValue.ReportUser;
        }

        [HttpGet("TimeEntries")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TimeEntriesResponseDto>> FetchTimeEntries(DateTime fromDateInclusive, DateTime toDateInclusive)
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
        public ActionResult<List<TimeEntriesResponseDto>> UpsertTimeEntry([FromBody] List<CreateTimeEntryDto> requests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values);
            }

            var user = _userRetriever.RetrieveUser();

            return Ok(_creator.UpsertTimeEntry(requests, user.Id, user.StartDate)
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

        [HttpGet("TimeEntriesReport")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TimeEntriesResponseDto>> FetchTimeEntriesReport(DateTime fromDateInclusive, DateTime toDateInclusive)
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
    }
}
