using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.TimeEntries
{
    [Route("api/user")]
    [ApiController]
    public class TimeEntriesController : Controller
    {
        private readonly AlvTime_dbContext _database;

        public TimeEntriesController(AlvTime_dbContext database)
        {
            _database = database;
        }

        [HttpGet("TimeEntries")]
        [Authorize]
        public ActionResult<IEnumerable<TimeEntriesResponseDto>> FetchTimeEntries(DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            try
            {
                var user = RetrieveUser();

                var hours = _database.Hours
                    .Where(x => x.Date >= fromDateInclusive && x.Date <= toDateInclusive && x.User == user.Id)
                    .Select(x => new TimeEntriesResponseDto
                    {
                        Id = x.Id,
                        Value = x.Value,
                        Date = x.Date.Date,
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

        [HttpPost("TimeEntries")]
        [Authorize]
        public ActionResult<List<TimeEntriesResponseDto>> UpsertTimeEntry([FromBody] List<SaveHoursDto> requests)
        {
            List<TimeEntriesResponseDto> response = new List<TimeEntriesResponseDto>();
            
            foreach (var request in requests)
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

                    var responseDto = new TimeEntriesResponseDto
                    {
                        Id = timeEntry.Id,
                        Date = timeEntry.Date,
                        Value = timeEntry.Value,
                        TaskId = timeEntry.TaskId
                    };

                    response.Add(responseDto);
                }
                catch (Exception e)
                {
                    return BadRequest(new
                    {
                        Message = e.ToString()
                    });
                } 
            }

            return Ok(response);
        }

        private User RetrieveUser()
        {
            var username = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var email = User.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var alvUser = _database.User.FirstOrDefault(x => x.Email.Equals(email));

            return alvUser;
        }

        private Hours CreateNewTimeEntry(SaveHoursDto hoursDto, User user)
        {
            Hours hour = new Hours
            {
                Date = hoursDto.Date.Date,
                TaskId = hoursDto.TaskId,
                User = user.Id,
                Year = (short)hoursDto.Date.Year,
                DayNumber = (short)hoursDto.Date.DayOfYear
            };
            _database.Add(hour);
            return hour;
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
