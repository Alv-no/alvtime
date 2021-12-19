using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.AbsenseDays
{
    public class AbsenseDaysService : IAbsenseDaysService
    {
        private readonly ITimeRegistrationStorage _timeRegistrationStorage;
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
        private const int VacationDays = 25;

        // Current law says that you can take three 
        // calendar days in a row 4 times within a 12 month period
        private const int SickLeaveGroupAmount = 4;
        private const int SickLeaveGroupSize = 3;

        public AbsenseDaysService(ITimeRegistrationStorage timeRegistrationStorage, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            _timeRegistrationStorage = timeRegistrationStorage;
            _timeEntryOptions = timeEntryOptions;
        }

        public AbsenseDaysDto GetAbsenseDays(int userId, int year, DateTime? intervalStart)
        {
            IEnumerable<TimeEntriesResponseDto> sickLeaveDays = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = intervalStart ?? DateTime.Now.AddMonths(-12),
                ToDateInclusive = intervalStart.HasValue ? intervalStart.Value.AddMonths(12) : DateTime.Now,
                UserId = userId,
                TaskId = _timeEntryOptions.CurrentValue.SickDaysTask
            });

            return new AbsenseDaysDto
            {
                AbsenseDaysInAYear = SickLeaveGroupSize * SickLeaveGroupAmount,
                UsedAbsenseDays = CalculateUsedSickDays(sickLeaveDays.Where(day => day.Value > 0)),
            };
        }

        private IEnumerable<DateTime> GetAlvDays(RedDays redDays, int year)
        {
            // Get the amount of days in romjula that is not a wekkend day
            // The 3 represents the three days of easter. Alvdays removed after 2021
            if (year < 2022)
            {
                return redDays.Dates.Where(days => days.Month == 12 &&
                                                   days.Day > 26 &&
                                                   days.Day <= 31 &&
                                                   days.DayOfWeek != DayOfWeek.Saturday &&
                                                   days.DayOfWeek != DayOfWeek.Sunday).Concat(new List<DateTime> { redDays.GetMondayInEaster(year), redDays.GetTuesdayInEaster(year), redDays.GetWednesdayInEaster(year) });
            }

            return new List<DateTime>();
        }

        private int CalculateUsedSickDays(IEnumerable<TimeEntriesResponseDto> entries)
        {

            // Group by coherence
            var groups = FindGroupedSickDays(entries.OrderBy(en => en.Date));

            // If a grouping is larger than the set amount. Break those groups down
            for (int i = 0; i < groups.Count(); i++)
            {
                var group = groups.ElementAt(i);
                while (group.Count() > SickLeaveGroupSize)
                {
                    var newGroup = group.Take(SickLeaveGroupSize);
                    group = group.Skip(SickLeaveGroupSize);
                    groups = groups.Append(newGroup);
                }
            }

            return (groups.Count() * SickLeaveGroupSize);

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

            if (a.Year == b.Value.Year && (a.DayOfYear == b.Value.DayOfYear + 1 || a.DayOfYear == b.Value.DayOfYear - 1))
                return true;
            return false;
        }

        public VacationDaysDTO GetVacationDays(int userId, int year, int month, int day)
        {

            IEnumerable<TimeEntriesResponseDto> paidVacationEntries = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year, 01, 01),
                ToDateInclusive = new DateTime(year, 12, 31),
                UserId = userId,
                TaskId = _timeEntryOptions.CurrentValue.PaidHolidayTask
            });

            IEnumerable<TimeEntriesResponseDto> unpaidVacationEntries = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = new DateTime(year, 01, 01),
                ToDateInclusive = new DateTime(year, 12, 31),
                UserId = userId,
                TaskId = _timeEntryOptions.CurrentValue.UnpaidHolidayTask
            });

            var redDays = new RedDays(year);

            var now = new DateTime(year, month, day);

            var vacationTransactions = paidVacationEntries.Concat(unpaidVacationEntries);
            var alvdays = GetAlvDays(redDays, year);

            var plannedVacation = vacationTransactions.Where(item => item.Value > 0 && item.Date.CompareTo(now) > 0);
            var plannedVacationExcludingAlvdays = plannedVacation.Where(item => !alvdays.Contains(item.Date));
            var usedVacation = vacationTransactions.Where(item => item.Value > 0 && item.Date.CompareTo(now) <= 0);

            var usedAlvdays = alvdays.Where(item => item.CompareTo(now) < 0);
            var usedVacationExcludingAlvdays = usedVacation.Select(used => used.Date).Where(u => !usedAlvdays.Contains(u));

            return new VacationDaysDTO
            {
                PlannedVacationDays = plannedVacationExcludingAlvdays.Count() + alvdays.Count() - usedAlvdays.Count(),
                UsedVacationDays = usedVacationExcludingAlvdays.Count() + usedAlvdays.Count(),
                AvailableVacationDays = VacationDays - plannedVacationExcludingAlvdays.Count() - usedVacationExcludingAlvdays.Count(),
                PlannedTransactions = plannedVacation,
                UsedTransactions = usedVacation
            };
        }
    }
}
