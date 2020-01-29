using AlvTimeApi.Controllers.Tasks;
using AlvTimeApi.DataBaseModels;
using AlvTimeApi.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker1.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class TimeEntriesController : Controller
    {
        private readonly ApplicationDbContext _database;

        public TimeEntriesController(ApplicationDbContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Retrieves time entries for the dates entered
        /// </summary>
        /// <remarks>Enter date in format yyyy-mm-dd</remarks>
        /// <response code="200">OK</response>
        [HttpGet("TimeEntries")]
        public ActionResult<IEnumerable<TimeEntriesResponseDto>> FetchTimeEntries(DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            try
            {
                var user = RetrieveUser();

                var hours = _database.Hours
                    .Where(x => x.Date >= fromDateInclusive && x.Date <= toDateInclusive && x.User == user.Id)
                    .Select(x => new TimeEntriesResponseDto
                    {
                        Value = x.Value,
                        Date = x.Date,
                        TaskId = x.TaskId
                    })
                    .ToList();

                return Ok(hours);
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.ToString(),
                });
            }
        }

        /// <summary>
        /// Updates existing or creates new time entry
        /// </summary>
        /// <remarks>Enter date in format yyyy-mm-dd</remarks>
        /// <response code="200">OK</response>
        [HttpPost("TimeEntries")]
        public ActionResult<TimeEntriesResponseDto> UpsertTimeEntry([FromBody] SaveHoursDto request)
        {
            try
            {
                var user = RetrieveUser();
                Hours timeEntry = RetrieveExistingTimeEntry(request, user);
                if (timeEntry == null)
                {
                    timeEntry = CreateNewTimeEntry(request, user);
                }

                timeEntry.Value = request.Value;
                _database.SaveChanges();

                return Ok(_database.Hours.FirstOrDefault(h => h.Id == timeEntry.Id));
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.ToString()
                });
            }
        }

        private User RetrieveUser()
        {

            var username = HttpContext.User.Identity.Name ?? "NameNotFound";
            var user = _database.User.FirstOrDefault(x => x.Email.Trim() == username.Trim());

            return user;
        }

        private Hours CreateNewTimeEntry(SaveHoursDto hoursDto, User user)
        {
            Hours ha = new Hours
            {
                Date = hoursDto.Date,
                TaskId = hoursDto.TaskId,
                User = user.Id
            };
            _database.Add(ha);
            return ha;
        }

        private Hours RetrieveExistingTimeEntry(SaveHoursDto hoursDto, User user)
        {
            return _database.Hours.FirstOrDefault(h =>
                h.Date.Date == hoursDto.Date.Date &&
                h.TaskId == hoursDto.TaskId &&
                h.User == user.Id);
        }
    }
}
