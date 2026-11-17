using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Options;
using AlvTime.Business.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.Absence;

public class AbsencePlanService(
    IOptionsMonitor<TimeEntryOptions> timeEntryOptions,
    IUserContext userContext,
    ITaskStorage taskStorage,
    ITimeRegistrationStorage timeRegistrationStorage,
    TimeRegistrationService timeRegistrationService,
    UserService userService)
{
    private const decimal HoursInWorkday = 7.5M;

    public async Task<List<AbsencePlanTaskDto>> GetAbsencePlanTasks()
    {
        var options = timeEntryOptions.CurrentValue;
        var configuredIds = options.AbsencePlanTasks;

        var tasks = await taskStorage.GetTasks(new TaskQuerySearch { Locked = false });

        return tasks
            .Where(t => configuredIds.Contains(t.Id) && t.Id != options.FlexTask)
            .OrderBy(t => t.Id != options.PaidHolidayTask)
            .ThenBy(t => configuredIds.IndexOf(t.Id))
            .Select(t => new AbsencePlanTaskDto
            {
                Id = t.Id,
                Name = t.Name,
                IsPaidHoliday = t.Id == options.PaidHolidayTask
            })
            .ToList();
    }

    public async Task<Result<AbsencePlanDto>> GetPlan(AbsencePlanRequest request)
    {
        var validationErrors = await ValidateRequest(request);
        if (validationErrors.Count != 0)
        {
            return validationErrors;
        }

        var user = await userContext.GetCurrentUser();
        var from = request.FromDateInclusive.Date;
        var to = request.ToDateInclusive.Date;

        var entriesByDate = (await timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = user.Id,
                FromDateInclusive = from,
                ToDateInclusive = to
            }))
            .GroupBy(e => e.Date.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var redDays = Enumerable.Range(from.Year, to.Year - from.Year + 1)
            .SelectMany(year => new RedDays(year).Dates)
            .ToHashSet();

        var days = new List<AbsencePlanDayDto>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var entriesOnDate = entriesByDate.GetValueOrDefault(date) ?? [];
            days.Add(await PlanDay(request, user.Id, date, entriesOnDate, redDays));
        }

        var registerDays = days.Where(d => d.Status == AbsencePlanDayStatus.Register).ToList();
        return new AbsencePlanDto
        {
            Days = days,
            TotalDays = registerDays.Count,
            TotalHours = registerDays.Sum(d => d.Hours)
        };
    }

    public async Task<Result<AbsencePlanDto>> RegisterPlan(AbsencePlanRequest request)
    {
        var planResult = await GetPlan(request);
        if (!planResult.IsSuccess)
        {
            return planResult.Errors;
        }

        var plan = planResult.Value;
        var entriesToRegister = plan.Days
            .Where(d => d.Status == AbsencePlanDayStatus.Register)
            .Select(d => new CreateTimeEntryDto
            {
                Date = d.Date,
                Value = d.Hours,
                TaskId = request.TaskId,
                Comment = request.Comment
            })
            .ToList();

        if (entriesToRegister.Count == 0)
        {
            return plan;
        }

        var upsertResult = await timeRegistrationService.UpsertTimeEntry(entriesToRegister);
        if (!upsertResult.IsSuccess)
        {
            return upsertResult.Errors;
        }

        return plan;
    }

    private async Task<AbsencePlanDayDto> PlanDay(AbsencePlanRequest request, int userId, DateTime date,
        List<TimeEntryResponseDto> entriesOnDate, HashSet<DateTime> redDays)
    {
        var day = new AbsencePlanDayDto { Date = date };

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            day.Status = AbsencePlanDayStatus.Weekend;
            return day;
        }

        if (redDays.Contains(date))
        {
            day.Status = AbsencePlanDayStatus.RedDay;
            return day;
        }

        var employmentRate = await userService.GetCurrentEmploymentRateForUser(userId, date);
        if (!employmentRate.IsSuccess)
        {
            day.Status = AbsencePlanDayStatus.Blocked;
            day.Reason = employmentRate.Errors.First().Description;
            return day;
        }

        var fullDay = HoursInWorkday * employmentRate.Value;
        var hours = RoundToQuarterHour(fullDay * request.Percentage / 100M);

        var existingOnTask = entriesOnDate.Where(e => e.TaskId == request.TaskId).Sum(e => e.Value);
        var otherHours = entriesOnDate.Where(e => e.TaskId != request.TaskId).Sum(e => e.Value);
        day.ExistingOtherHours = otherHours;

        if (otherHours > 0)
        {
            hours = request.FillPartialDays ? Math.Min(hours, fullDay - otherHours) : 0;
            if (hours <= 0)
            {
                day.Status = AbsencePlanDayStatus.HasOtherHours;
                return day;
            }
        }

        if (existingOnTask == hours)
        {
            day.Status = AbsencePlanDayStatus.AlreadyRegistered;
            return day;
        }

        var errors = await timeRegistrationService.ValidateTimeEntry(new CreateTimeEntryDto
        {
            Date = date,
            Value = hours,
            TaskId = request.TaskId
        });
        if (errors.Count != 0)
        {
            day.Status = AbsencePlanDayStatus.Blocked;
            day.Reason = errors.First().Description;
            return day;
        }

        day.Status = AbsencePlanDayStatus.Register;
        day.Hours = hours;
        return day;
    }

    private async Task<List<Error>> ValidateRequest(AbsencePlanRequest request)
    {
        var errors = new List<Error>();
        var allowedTasks = await GetAbsencePlanTasks();

        if (allowedTasks.All(t => t.Id != request.TaskId))
        {
            errors.Add(new Error(ErrorCodes.RequestInvalidProperty, "Fraværskoden kan ikke registreres for en periode."));
        }

        if (request.FromDateInclusive.Date > request.ToDateInclusive.Date)
        {
            errors.Add(new Error(ErrorCodes.RequestInvalidProperty, "Fra-dato må være før til-dato."));
        }
        else if (request.ToDateInclusive.Date > request.FromDateInclusive.Date.AddYears(1))
        {
            errors.Add(new Error(ErrorCodes.RequestInvalidProperty, "Perioden kan ikke være lengre enn ett år."));
        }

        if (request.Percentage <= 0 || request.Percentage > 100)
        {
            errors.Add(new Error(ErrorCodes.RequestInvalidProperty, "Andel av dagen må være mellom 1 og 100 prosent."));
        }

        return errors;
    }

    private static decimal RoundToQuarterHour(decimal hours) =>
        Math.Round(hours * 4, MidpointRounding.AwayFromZero) / 4;
}
