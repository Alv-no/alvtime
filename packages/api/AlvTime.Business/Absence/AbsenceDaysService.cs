using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.Absence;

public class AbsenceDaysService : IAbsenceDaysService
{
    private readonly ITimeRegistrationStorage _timeRegistrationStorage;
    private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
    private readonly IUserContext _userContext;
    private const int VacationDays = 25;
    private const decimal DaysInYear = 365.2425M;

    // Current law says that you can take three 
    // calendar days in a row 4 times within a 12 month period
    private const int SickLeaveGroupAmount = 4;
    private const int SickLeaveGroupSize = 3;

    public AbsenceDaysService(ITimeRegistrationStorage timeRegistrationStorage,
        IOptionsMonitor<TimeEntryOptions> timeEntryOptions, IUserContext userContext)
    {
        _timeRegistrationStorage = timeRegistrationStorage;
        _timeEntryOptions = timeEntryOptions;
        _userContext = userContext;
    }

    public AbsenceDaysDto GetAbsenceDays(int userId, int year, DateTime? intervalStart)
    {
        IEnumerable<TimeEntryResponseDto> sickLeaveDays = _timeRegistrationStorage.GetTimeEntries(
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
                                               days.DayOfWeek != DayOfWeek.Sunday).Concat(new List<DateTime>
            {
                redDays.GetMondayInEaster(year), redDays.GetTuesdayInEaster(year), redDays.GetWednesdayInEaster(year)
            });
        }

        return new List<DateTime>();
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

    private bool CoherentDates(DateTime a, DateTime? b)
    {
        if (!b.HasValue)
            return false;

        if (a.Year == b.Value.Year && (a.DayOfYear == b.Value.DayOfYear + 1 || a.DayOfYear == b.Value.DayOfYear - 1))
            return true;
        return false;
    }

    public VacationDaysDTO GetAllTimeVacationOverview(int currentYear)
    {
        var user = _userContext.GetCurrentUser();
        var userStartDate = user.StartDate;

        var paidVacationEntries = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = userStartDate,
            ToDateInclusive = new DateTime(currentYear, 12, 31),
            UserId = user.Id,
            TaskId = _timeEntryOptions.CurrentValue.PaidHolidayTask
        });

        var unpaidVacationEntries = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = userStartDate,
            ToDateInclusive = new DateTime(currentYear, 12, 31),
            UserId = user.Id,
            TaskId = _timeEntryOptions.CurrentValue.UnpaidHolidayTask
        });

        var numberOfYearsWorked = currentYear - userStartDate.Year;
        var yearsWorked = new List<int>();
        for (var i = 0; i <= numberOfYearsWorked; i++)
        {
            yearsWorked.Add(userStartDate.Year + i);
        }

        var allRedDays = new List<string>();
        foreach (var year in yearsWorked)
        {
            var redDaysInYear = new RedDays(year).Dates.Select(d => d.ToShortDateString());
            allRedDays.AddRange(redDaysInYear);
        }

        var vacationTransactions = paidVacationEntries.Concat(unpaidVacationEntries).ToList();

        var now = DateTime.Now;

        var plannedVacation = vacationTransactions.Where(entry =>
                entry.Value > 0 && entry.Date.CompareTo(now) > 0 &&
                !allRedDays.Contains(entry.Date.ToShortDateString()))
            .ToList();
        var usedVacation = vacationTransactions.Where(entry =>
                entry.Value > 0 && entry.Date.CompareTo(now) <= 0 &&
                !allRedDays.Contains(entry.Date.ToShortDateString()))
            .ToList();

        var usedVacationThisYear = usedVacation.Where(uv => uv.Date.Year == DateTime.Now.Year);
        var allSpentVacation = plannedVacation.Concat(usedVacation);
        var spentVacationByYear = allSpentVacation.GroupBy(u => u.Date.Year).ToList();
        
        var usersAvailableVacationDays = 0;
        
        var startDay = userStartDate.DayOfYear;

        foreach (var year in yearsWorked)
        {
            var daysWorkedLastYear = userStartDate.Year == year ? 0 :
                userStartDate.Year == year - 1 ? DaysInYear - startDay : DaysInYear;
            
            var earnedDaysUpToThisYear = userStartDate.Year == year ? 0 : (int)Math.Round(daysWorkedLastYear * (VacationDays / DaysInYear));

            var spentVacationThisYear = spentVacationByYear.Where(v => v.Key == year).SelectMany(v => v).Count();
            usersAvailableVacationDays += Math.Max(earnedDaysUpToThisYear - spentVacationThisYear, 0);
        }

        return new VacationDaysDTO
        {
            PlannedVacationDays = plannedVacation.Count,
            UsedVacationDays = usedVacationThisYear.Count(),
            AvailableVacationDays = usersAvailableVacationDays,
            PlannedTransactions = plannedVacation,
            UsedTransactions = usedVacation
        };
    }
}