using System;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;

namespace AlvTime.Persistence.Repositories
{
    public class TimeRegistrationStorage : ITimeRegistrationStorage
    {
        private readonly AlvTime_dbContext _context;

        public TimeRegistrationStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public IEnumerable<TimeEntryWithCompRateDto> GetTimeEntriesWithCompensationRate(TimeEntryQuerySearch criterias)
        {
            var timeEntries = GetTimeEntries(criterias);
            var compensationRates = _context.CompensationRate.ToList();
            var timeEntriesWithCompensationRate =
                timeEntries.GroupJoin(compensationRates, timeEntry => timeEntry.TaskId, rate => rate.TaskId,
                    (dto, rates) => new TimeEntryWithCompRateDto
                    {
                        CompensationRate = rates.OrderByDescending(rate => rate.FromDate)
                            .First(x => x.FromDate.Date <= dto.Date.Date).Value,
                        Id = dto.Id,
                        Date = dto.Date,
                        User = dto.User,
                        Value = dto.Value,
                        TaskId = dto.TaskId,
                    });

            return timeEntriesWithCompensationRate;
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
                        CompensationRate = compensationRates.FirstOrDefault(cr => cr.TaskId == e.TaskId)?.Value ?? 1M,
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
            var task = _context.Task
                .FirstOrDefault(t => t.Id == timeEntry.TaskId);

            if (!task.Locked)
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

                var user = _context.User.FirstOrDefault(u => u.Id == hour.User);
                return new TimeEntriesResponseDto
                {
                    Id = hour.Id,
                    User = hour.User,
                    Date = hour.Date,
                    TaskId = hour.TaskId,
                    Value = hour.Value,
                    UserEmail = user?.Email
                };
            }

            throw new Exception("Cannot create time entry. Task is locked");
        }

        public TimeEntriesResponseDto UpdateTimeEntry(CreateTimeEntryDto timeEntry, int userId)
        {
            var hour = _context.Hours.AsQueryable()
                .Filter(new TimeEntryQuerySearch
                {
                    TaskId = timeEntry.TaskId,
                    FromDateInclusive = timeEntry.Date.Date,
                    ToDateInclusive = timeEntry.Date.Date,
                    UserId = userId
                })
                .FirstOrDefault();

            var task = _context.Task
                .FirstOrDefault(t => t.Id == timeEntry.TaskId);

            if (!hour.Locked && !task.Locked)
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

            throw new Exception("Cannot update time entry. Task or time entry is locked");
        }

        public List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias)
        {
            var overtimeEntries = _context.EarnedOvertime.AsQueryable()
                .Filter(criterias)
                .Select(entry => new EarnedOvertimeDto
                {
                    Date = entry.Date,
                    Value = entry.Value,
                    CompensationRate = entry.CompensationRate,
                    UserId = entry.UserId
                })
                .ToList();
            return overtimeEntries;
        }

        public void StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId)
        {
            _context.EarnedOvertime.AddRange(overtimeEntries.Select(entry => new EarnedOvertime
            {
                Date = entry.Date,
                UserId = userId,
                Value = entry.Hours,
                CompensationRate = entry.CompensationRate
            }));
            _context.SaveChanges();
        }

        public virtual void DeleteOvertimeOnDate(DateTime date, int userId)
        {
            var earnedOvertimeOnDate =
                _context.EarnedOvertime.Where(ot => ot.Date.Date == date.Date && ot.UserId == userId);
            _context.RemoveRange(earnedOvertimeOnDate);
            _context.SaveChanges();
        }
    }
}