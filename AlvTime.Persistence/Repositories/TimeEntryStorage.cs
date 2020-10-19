using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTime.Persistence.Repositories
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
                        User = x.User,
                        Value = x.Value,
                        Date = x.Date,
                        TaskId = x.TaskId
                    })
                    .ToList();

            foreach (var entry in hours)
            {
                entry.UserEmail = _context.User.FirstOrDefault(x => x.Id == entry.User).Email;
            }

            return hours;
        }

        public IEnumerable<DateEntry> GetDateEntries(TimeEntryQuerySearch criterias)
        {
            var hours = _context.Hours
                .Include(h => h.Task)
                .AsQueryable()
                .Filter(criterias)
                .ToList();

            var compensationRates = _context.CompensationRate.OrderByDescending(cr => cr.FromDate);

            return hours.GroupBy(
                entry => entry.Date,
                entry => entry,
                (date, entry) => new DateEntry
                {
                    Date = date,
                    Entries = entry.Select(e => new Entry
                    {
                        TaskId = e.TaskId,
                        Value = e.Value,
                        CompensationRate = compensationRates.FirstOrDefault(cr => cr.TaskId == e.TaskId).Value,
                    })
                });
        }

        public TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias)
        {
            var timeEntry = _context.Hours.AsQueryable()
                .Filter(criterias)
                .Select(x => new TimeEntriesResponseDto
                {
                    Id = x.Id,
                    Value = x.Value,
                    Date = x.Date,
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
                User = hour.User,
                Date = hour.Date,
                TaskId = hour.TaskId,
                Value = hour.Value,
                UserEmail = hour.UserNavigation.Email
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
                .Where(t => t.Id == timeEntry.TaskId)
                .FirstOrDefault();

            if (hour.Locked == false && task.Locked == false)
            {
                hour.Value = timeEntry.Value;
                _context.SaveChanges();

                return new TimeEntriesResponseDto
                {
                    Id = hour.Id,
                    User = hour.User,
                    Date = hour.Date,
                    TaskId = hour.TaskId,
                    Value = hour.Value,
                    UserEmail = hour.UserNavigation.Email
                };
            }

            return new TimeEntriesResponseDto();
        }
    }
}
