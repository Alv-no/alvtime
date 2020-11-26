using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private readonly int TEST_USER = 11;
        private readonly int REPORT_USER = 17;

        public TimeEntriesController(RetrieveUsers userRetriever, ITimeEntryStorage storage, TimeEntryCreator creator)
        {
            _userRetriever = userRetriever;
            _storage = storage;
            _creator = creator;
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
            var user = _userRetriever.RetrieveUser();

            return Ok(_creator.UpsertTimeEntry(requests, user.Id)
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

            if (user.Id == TEST_USER || user.Id == REPORT_USER)
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
