using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Tasks;
using AlvTime.Business.TimeEntries;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.Overtime
{
    public class OvertimeService
    {
        private readonly IOvertimeStorage _overtimeStorage;
        private readonly IUserContext _userContext;
        private readonly ITaskStorage _taskStorage;
        private readonly int _absenceProjectId;
        
        private const decimal HOURS_IN_WORKDAY = 7.5M;

        public OvertimeService(
            IOvertimeStorage overtimeStorage, 
            IUserContext userContext, 
            ITaskStorage taskStorage, 
            IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            _overtimeStorage = overtimeStorage;
            _userContext = userContext;
            _taskStorage = taskStorage;
            _absenceProjectId = timeEntryOptions.CurrentValue.AbsenceProject;
        }

        public List<OvertimeEntry> StoreNewOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
        {
            var currentUser = _userContext.GetCurrentUser();
            
            var timeEntryDate = timeEntriesOnDay.First().Date.Date;
            var allRedDays = new RedDays(timeEntryDate.Year).Dates;
            
            var anticipatedWorkHours =
                IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate) ? 0 : HOURS_IN_WORKDAY;
            var normalWorkHoursLeft = anticipatedWorkHours;
            var overtimeEntries = new List<OvertimeEntry>();
            foreach (var timeEntry in timeEntriesOnDay.OrderByDescending(entry => entry.CompensationRate))
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
            
            _overtimeStorage.StoreOvertime(overtimeEntries, currentUser.Id);

            return overtimeEntries;
        }

        public List<OvertimeEntry> UpdateEarnedOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
        {
            var currentUser = _userContext.GetCurrentUser();
            var timeEntryDate = timeEntriesOnDay.First().Date.Date;
            _overtimeStorage.DeleteOvertimeOnDate(timeEntryDate, currentUser.Id);

            return StoreNewOvertime(timeEntriesOnDay);
        }
        
        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
        
        private bool TaskGivesOvertime(int taskId)
        {
            var task = _taskStorage.GetTasks(new TaskQuerySearch{ Id = taskId }).FirstOrDefault();
            return task != null && task.Project.Id != _absenceProjectId;
        }
    }
}