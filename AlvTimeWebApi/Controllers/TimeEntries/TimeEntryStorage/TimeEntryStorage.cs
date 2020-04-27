using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.TimeEntries.TimeEntryStorage
{
    public class TimeEntryStorage : ITimeEntryStorage
    {

        private readonly AlvTime_dbContext _context;

        public TimeEntryStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public IEnumerable<TimeEntriesResponseDto> GetTimeEntries(TimeEntryQuerySearch criterias)
        {
            var hours = _context.Hours.AsQueryable()
                    .Filter(criterias)
                    .Select(x => new TimeEntriesResponseDto
                    {
                        Id = x.Id,
                        Value = x.Value,
                        Date = x.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        TaskId = x.TaskId
                    })
                    .ToList();

            return hours;
        }

        public TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias)
        {
            var timeEntry = _context.Hours.AsQueryable()
                .Filter(criterias)
                .Select(x => new TimeEntriesResponseDto
                {
                    Id = x.Id,
                    Value = x.Value,
                    Date = x.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TaskId = x.TaskId
                }).FirstOrDefault();

            return timeEntry;
        }

        public TimeEntriesResponseDto CreateTimeEntry(CreateTimeEntryDto timeEntry, int userId)
        {
            Hours hour = new Hours
            {
                Date = timeEntry.Date.Date,
                TaskId = timeEntry.TaskId,
                User = userId,
                Year = (short)timeEntry.Date.Year,
                DayNumber = (short)timeEntry.Date.DayOfYear,
                Value = timeEntry.Value
            };
            _context.Hours.Add(hour);
            _context.SaveChanges();

            return new TimeEntriesResponseDto
            {
                Id = hour.Id,
                Date = hour.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                TaskId = hour.TaskId,
                Value = hour.Value
            };
        }

        public TimeEntriesResponseDto UpdateTimeEntry(CreateTimeEntryDto timeEntry, int userId)
        {
            var hour = _context.Hours.AsQueryable()
                .Filter(new TimeEntryQuerySearch
                {
                    TaskId = timeEntry.TaskId,
                    FromDateInclusive = timeEntry.Date,
                    ToDateInclusive = timeEntry.Date,
                    UserId = userId
                })
                .FirstOrDefault();

            var task = _context.Task
                .Where(task => task.Id == timeEntry.TaskId)
                .FirstOrDefault();

            if(hour.Locked == false &&  task.Locked == false)
            {
                hour.Value = timeEntry.Value;
                _context.SaveChanges();

                return new TimeEntriesResponseDto
                {
                    Id = hour.Id,
                    Date = hour.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TaskId = hour.TaskId,
                    Value = hour.Value
                };
            }

            return new TimeEntriesResponseDto();
        }
    }
}
