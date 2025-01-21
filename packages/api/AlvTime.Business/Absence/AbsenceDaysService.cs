using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.Absence;

public class AbsenceDaysService : IAbsenceDaysService
{
    private readonly ITimeRegistrationStorage _timeRegistrationStorage;
    private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
    private readonly IUserContext _userContext;
    private readonly IAbsenceStorage _absenceStorage;
    private const int DefaultVacationDaysAmount = 25;
    private decimal _daysInYear = 365.2425M;

    // Current law says that you can take three 
    // calendar days in a row 4 times within a 12 month period
    private const int SickLeaveGroupAmount = 4;
    private const int SickLeaveGroupSize = 3;

    public AbsenceDaysService(ITimeRegistrationStorage timeRegistrationStorage,
        IOptionsMonitor<TimeEntryOptions> timeEntryOptions, IUserContext userContext, IAbsenceStorage absenceStorage)
    {
        _timeRegistrationStorage = timeRegistrationStorage;
        _timeEntryOptions = timeEntryOptions;
        _userContext = userContext;
        _absenceStorage = absenceStorage;
    }

    public async Task<AbsenceDaysDto> GetAbsenceDays(int userId, int year, DateTime? intervalStart)
    {
        IEnumerable<TimeEntryResponseDto> sickLeaveDays = await _timeRegistrationStorage.GetTimeEntries(
            new TimeEntryQuerySearch
            {
                FromDateInclusive = intervalStart ?? DateTime.Now.AddMonths(-12),
                ToDateInclusive = intervalStart.HasValue ? intervalStart.Value.AddMonths(12) : DateTime.Now,
                UserId = userId,
                TaskId = _timeEntryOptions.CurrentValue.SickDaysTask
            });

        return new AbsenceDaysDto
        {
            AbsenceDaysInAYear = SickLeaveGroupSize * SickLeaveGroupAmount,
            UsedAbsenceDays = CalculateUsedSickDays(sickLeaveDays.Where(day => day.Value > 0)),
        };
    }

    private int CalculateUsedSickDays(IEnumerable<TimeEntryResponseDto> entries)
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

    private IEnumerable<IEnumerable<TimeEntryResponseDto>> FindGroupedSickDays(
        IEnumerable<TimeEntryResponseDto> sickDays)
    {
        List<List<TimeEntryResponseDto>> groupings = new List<List<TimeEntryResponseDto>>();

        int cursor = 0;
        int currentGroup = 0;

        while (cursor < sickDays.Count())
        {
            if (groupings.ElementAtOrDefault(currentGroup) == null)
            {
                groupings.Add(new List<TimeEntryResponseDto>());
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

    private static bool CoherentDates(DateTime a, DateTime? b)
    {
        if (!b.HasValue)
            return false;

        if (a.Year == b.Value.Year && (a.DayOfYear == b.Value.DayOfYear + 1 || a.DayOfYear == b.Value.DayOfYear - 1))
            return true;
        return false;
    }

    public async Task<VacationDaysDTO> GetAllTimeVacationOverview(int currentYear)
    {
        var currentUser = await _userContext.GetCurrentUser();
        var currentUserStartDate = currentUser.StartDate;

        var paidVacationEntries = await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = currentUserStartDate,
            ToDateInclusive = new DateTime(currentYear, 12, 31),
            UserId = currentUser.Id,
            TaskId = _timeEntryOptions.CurrentValue.PaidHolidayTask
        });

        var unpaidVacationEntries = await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = currentUserStartDate,
            ToDateInclusive = new DateTime(currentYear, 12, 31),
            UserId = currentUser.Id,
            TaskId = _timeEntryOptions.CurrentValue.UnpaidHolidayTask
        });
        var allVacationTransactions = paidVacationEntries.Concat(unpaidVacationEntries).ToList();

        var numberOfYearsWorked = currentYear - currentUserStartDate.Year;
        var yearsWorked = new List<int>();
        var allRedDays = new List<string>();
        for (var i = 0; i <= numberOfYearsWorked; i++)
        {
            var year = currentUserStartDate.Year + i;
            yearsWorked.Add(year);
            var redDaysInYear = new RedDays(year).Dates.Select(d => d.ToShortDateString());
            allRedDays.AddRange(redDaysInYear);
        }

        var now = DateTime.Now;
        var plannedVacation = allVacationTransactions.Where(entry =>
                entry.Value > 0 && entry.Date.CompareTo(now) > 0 &&
                !allRedDays.Contains(entry.Date.ToShortDateString()))
            .ToList();
        var usedVacation = allVacationTransactions.Where(entry =>
                entry.Value > 0 && entry.Date.CompareTo(now) <= 0 &&
                !allRedDays.Contains(entry.Date.ToShortDateString()))
            .ToList();

        var usedVacationThisYear = usedVacation.Where(uv => uv.Date.Year == DateTime.Now.Year);
        var plannedVacationThisYear = plannedVacation.Where(pv => pv.Date.Year == DateTime.Now.Year);
        var allSpentVacation = plannedVacation.Concat(usedVacation);
        var spentVacationByYear = allSpentVacation.GroupBy(u => u.Date.Year).ToList();

        var usersAvailableVacationDays = 0M;

        var userStartDay = currentUserStartDate.DayOfYear;

        var overridenVacation = (await _absenceStorage.GetCustomVacationEarned(currentUser.Id)).ToList();

        var usersAvailableVacationDaysThisYear = 0M;

        foreach (var year in yearsWorked)
        {
            if (DateTime.IsLeapYear(year))
            {
                _daysInYear = 366.2425M;
            }
            
            var daysEmployedLastYear = currentUserStartDate.Year == year ? 0 :
                currentUserStartDate.Year == year - 1 ? _daysInYear - userStartDay : _daysInYear;

            var earnedDaysFromPreviousYear = overridenVacation.Any(v => v.Year == year - 1)
                ? overridenVacation.Single(v => v.Year == year - 1).DaysEarned
                : (int)Math.Round(daysEmployedLastYear * (DefaultVacationDaysAmount / _daysInYear));

            var spentVacationThisYear =
                spentVacationByYear.Where(v => v.Key == year).SelectMany(v => v).Sum(v => v.Value) / 7.5M;

            var sumVacationDaysThisYear = earnedDaysFromPreviousYear - spentVacationThisYear;
            if (sumVacationDaysThisYear < 0)
            {
                usersAvailableVacationDays -= Math.Min(Math.Abs(sumVacationDaysThisYear), usersAvailableVacationDays);
            }
            else
            {
                usersAvailableVacationDays += sumVacationDaysThisYear;
            }

            if (year == currentYear)
            {
                usersAvailableVacationDaysThisYear = earnedDaysFromPreviousYear;
            }
        }

        return new VacationDaysDTO
        {
            PlannedVacationDaysThisYear = plannedVacationThisYear.Sum(v => v.Value) / 7.5M,
            UsedVacationDaysThisYear = usedVacationThisYear.Sum(v => v.Value) / 7.5M,
            AvailableVacationDays = usersAvailableVacationDays,
            AvailableVacationDaysTransferredFromLastYear = Math.Max(0, usersAvailableVacationDays - usersAvailableVacationDaysThisYear),
            PlannedTransactions = plannedVacation,
            UsedTransactions = usedVacation
        };
    }
}