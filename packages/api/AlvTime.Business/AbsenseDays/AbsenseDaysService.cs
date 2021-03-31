using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.AbsenseDays
{
    public class AbsenseDaysService : IAbsenseDaysService
    {
        private readonly ITimeEntryStorage timeEntryStorage;
        private readonly IOptionsMonitor<TimeEntryOptions> timeEntryOptions;
        private const int vacationDays = 25;

        // Current law says that you can take three 
        // calendar days in a row 4 times within a 12 month period
        private const int sickLeaveGroupAmount = 4;
        private const int sickLeaveGroupSize = 3;


        public AbsenseDaysService(ITimeEntryStorage timeEntryStorage, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            this.timeEntryStorage = timeEntryStorage;
            this.timeEntryOptions = timeEntryOptions;
        }

        public AbsenseDaysDto GetAbsenseDays(int userId, int year, DateTime? intervalStart)
        {
            IEnumerable<TimeEntriesResponseDto> vacationEntries = timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year, 01, 01),
                ToDateInclusive = new DateTime(year, 12, 31),
                UserId = userId,
                TaskId = timeEntryOptions.CurrentValue.PaidHolidayTask
            });

            IEnumerable<TimeEntriesResponseDto> sickLeaveDays = timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = intervalStart ?? DateTime.Now.AddMonths(-12),
                ToDateInclusive = intervalStart.HasValue ? intervalStart.Value.AddMonths(12) : DateTime.Now,
                UserId = userId,
                TaskId = timeEntryOptions.CurrentValue.SickDaysTask
            });

            return new AbsenseDaysDto
            {
                PayedVacationDaysLeft = vacationDays - vacationEntries.Where(d => d.Value > 0).Count(),
                SickLeaveDaysLeft = CalculateRemainingSickDays(sickLeaveDays)
            };
        }

        private int CalculateRemainingSickDays(IEnumerable<TimeEntriesResponseDto> entries) {

            // Group by coherence
            var groups = FindGroupedSickDays(entries.OrderBy(en => en.Date));

            // If a grouping is larger than the set amount. Break those groups down
            for (int i = 0; i < groups.Count(); i++)
            {
                var group = groups.ElementAt(i);
                while (group.Count() > sickLeaveGroupSize)
                {
                    var newGroup = group.Take(sickLeaveGroupSize);
                    group = group.Skip(sickLeaveGroupSize);
                    groups = groups.Append(newGroup);
                }
            }

            return (sickLeaveGroupSize * sickLeaveGroupAmount) - (groups.Count() * sickLeaveGroupSize);

        }

        private IEnumerable<IEnumerable<TimeEntriesResponseDto>> FindGroupedSickDays(IEnumerable<TimeEntriesResponseDto> sickDays)
        {
            List<List<TimeEntriesResponseDto>> groupings = new List<List<TimeEntriesResponseDto>>();

            int cursor = 0;
            int currentGroup = 0;

            while (cursor < sickDays.Count())
            {
                if (groupings.ElementAtOrDefault(currentGroup) == null)
                {
                    groupings.Add(new List<TimeEntriesResponseDto>());
                    groupings.ElementAt(currentGroup).Add(sickDays.ElementAt(cursor));
                }

                if (CoherentDates(sickDays.ElementAt(cursor).Date, sickDays.ElementAtOrDefault(++cursor)?.Date))
                {
                    groupings.ElementAt(currentGroup).Add(sickDays.ElementAt(cursor));
                }
                else
                {
                    currentGroup++;
                }
            }

            return groupings;
        }

        private bool CoherentDates(DateTime a, DateTime? b)
        {
            if (!b.HasValue)
                return false;

            if (a.Year == b.Value.Year && (a.DayOfYear == b.Value.DayOfYear + 1 || a.DayOfYear == b.Value.DayOfYear -1))
                return true;
            return false;
        }
    }
}
