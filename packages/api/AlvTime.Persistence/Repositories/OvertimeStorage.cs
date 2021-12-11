using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.Extensions.Options;

namespace AlvTime.Persistence.Repositories
{
    public class OvertimeStorage : IOvertimeStorage
    {
        private readonly AlvTime_dbContext _context;
        private readonly ITimeEntryStorage _timeEntryStorage;
        private const decimal HOURS_IN_WORKDAY = 7.5M;
        private readonly int _absenceProject;

        public OvertimeStorage(AlvTime_dbContext context, IOptionsMonitor<TimeEntryOptions> timeEntryOptions, ITimeEntryStorage timeEntryStorage)
        {
            _context = context;
            _timeEntryStorage = timeEntryStorage;
            _absenceProject = timeEntryOptions.CurrentValue.AbsenceProject;
        }
        
        public void StoreOvertime(DateTime timeEntryDate, int userId)
        {
            timeEntryDate = timeEntryDate.Date;
            var allRedDays = new RedDays(timeEntryDate.Year).Dates;
            var timeEntriesForDay = _timeEntryStorage.GetTimeEntriesWithCompensationRate(new TimeEntryQuerySearch
            {
                UserId = userId,
                FromDateInclusive = timeEntryDate,
                ToDateInclusive = timeEntryDate
            });
            
            var anticipatedWorkHours =
                IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate) ? 0 : HOURS_IN_WORKDAY;
            var normalWorkHoursLeft = anticipatedWorkHours;
            var overtimeEntries = new List<OvertimeEntry>();
            foreach (var timeEntry in timeEntriesForDay.OrderByDescending(entry => entry.CompensationRate))
            {
                if (anticipatedWorkHours == 0 && !TaskGivesOvertime(timeEntry.TaskId))
                {
                    continue;
                }
                if (normalWorkHoursLeft <= 0)
                {
                    overtimeEntries.Add(new OvertimeEntry
                    {
                        Date = timeEntry.Date,
                        Hours = timeEntry.Value,
                        CompensationRate = timeEntry.CompensationRate,
                        TaskId = timeEntry.TaskId
                    });
                }
                if (normalWorkHoursLeft - timeEntry.Value < 0) //Split entry
                {
                    overtimeEntries.Add(new OvertimeEntry
                    {
                        Date = timeEntry.Date,
                        Hours = timeEntry.Value - normalWorkHoursLeft,
                        CompensationRate = timeEntry.CompensationRate,
                        TaskId = timeEntry.TaskId
                    });
                }

                normalWorkHoursLeft -= timeEntry.Value;
            }
        }
        
        private bool TaskGivesOvertime(int taskId)
        {
            var task = _context.Task.FirstOrDefault(task => task.Id == taskId);
            return task != null && task.Project != _absenceProject;
        }
        
        private bool WorkedOnRedDay(DateEntry day, List<DateTime> redDays)
        {
            if ((IsWeekend(day) ||
                 redDays.Contains(day.Date)) &&
                day.GetWorkingHours() > 0)
            {
                return true;
            }
            return false;
        }
        
        private bool IsWeekend(DateEntry entry)
        {
            return entry.Date.DayOfWeek == DayOfWeek.Saturday || entry.Date.DayOfWeek == DayOfWeek.Sunday;
        }
        
        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}