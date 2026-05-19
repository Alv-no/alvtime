using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.CompensationRate;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories;

public class TimeRegistrationStorage(AlvTime_dbContext context) : ITimeRegistrationStorage
{
    public async Task<IEnumerable<TimeEntry>> GetFlexEntries(TimeEntryQuerySearch criteria)
    {
        var entries = await context.RegisteredFlex.AsQueryable()
            .Filter(criteria)
            .Select(x => new TimeEntry
            {
                Date = x.Date,
                Hours = x.Value,
                CompensationRate = x.CompensationRate,
            }).ToListAsync();
        return entries;
    }
    
    public async Task RegisterFlex(TimeEntry timeEntry, int userId)
    {
        var flexEntry = new RegisteredFlex
        {
            Date = timeEntry.Date,
            UserId = userId,
            Value = timeEntry.Hours,
            CompensationRate = timeEntry.CompensationRate
        };
        await context.RegisteredFlex.AddAsync(flexEntry);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFlexOnDate(DateTime date, int userId)
    {
        var flexOnDate =
            context.RegisteredFlex.Where(ot => ot.Date.Date == date.Date && ot.UserId == userId);
        context.RemoveRange(flexOnDate);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TimeEntryWithCompRateDto>> GetTimeEntriesWithCompensationRate(TimeEntryQuerySearch criteria)
    {
        var entries = await context.Hours.AsQueryable()
            .Include(h => h.UserNavigation)
            .Filter(criteria)
            .Join(context.Task, h => h.TaskId, t => t.Id,
                (h, t) => new
                {
                    h.Id,
                    h.User,
                    h.Date,
                    h.Value,
                    h.TaskId,
                    t.CompensationType,
                    t.Imposed,
                    h.UserNavigation
                })
            .ToListAsync();

        return entries.Select(x => new TimeEntryWithCompRateDto
        {
            Id = x.Id,
            User = x.User,
            Date = x.Date,
            Value = x.Value,
            TaskId = x.TaskId,
            CompensationRate = CompensationRateHelper.ResolveCompensationRate(x.CompensationType, x.Imposed, x.UserNavigation.SalaryModel)
        });
    }
    
    public async Task<IEnumerable<TimeEntryResponseDto>> GetTimeEntries(TimeEntryQuerySearch criteria)
    {
        var userEmail = (await context.User.FirstOrDefaultAsync(x => x.Id == criteria.UserId))?.Email;
        var hours = await context.Hours.AsQueryable()
            .Filter(criteria)
            .Select(x => new TimeEntryResponseDto
            {
                Id = x.Id,
                User = x.User,
                Value = x.Value,
                Date = x.Date,
                TaskId = x.TaskId,
                Comment = x.Comment,
                CommentedAt = x.CommentedAt,
                UserEmail = userEmail
            })
            .ToListAsync();

        return hours;
    }

    public async Task<IEnumerable<TimeEntryResponseDto>> GetTimeEntriesReport(TimeEntryQuerySearch criteria)
    {
        var hours = (await GetTimeEntries(criteria)).ToList();

        foreach (var entry in hours)
        {
            entry.UserEmail = (await context.User.FirstOrDefaultAsync(x => x.Id == entry.User))?.Email;
        }

        return hours;
    }

    public async Task<TimeEntryResponseDto> GetTimeEntry(TimeEntryQuerySearch criteria)
    {
        var timeEntry = await context.Hours.AsQueryable()
            .Filter(criteria)
            .Select(x => new TimeEntryResponseDto
            {
                Id = x.Id,
                Value = x.Value,
                Date = x.Date,
                TaskId = x.TaskId,
                Comment = x.Comment,
                CommentedAt = x.CommentedAt
            }).FirstOrDefaultAsync();

        return timeEntry;
    }

    public async Task<TimeEntryResponseDto> CreateTimeEntry(CreateTimeEntryDto timeEntry, int userId)
    {
        var task = await context.Task
            .FirstOrDefaultAsync(t => t.Id == timeEntry.TaskId);

        if (!task.Locked)
        {
            var hour = new Hours
            {
                Date = timeEntry.Date.Date,
                TaskId = timeEntry.TaskId,
                User = userId,
                Year = (short)timeEntry.Date.Year,
                DayNumber = (short)timeEntry.Date.DayOfYear,
                Value = timeEntry.Value,
                TimeRegistered = DateTime.UtcNow
            };
            context.Hours.Add(hour);
            await context.SaveChangesAsync();
            await UpdateComment(timeEntry.Comment, hour.Id);

            var user = await context.User.FirstOrDefaultAsync(u => u.Id == hour.User);
            return new TimeEntryResponseDto
            {
                Id = hour.Id,
                User = hour.User,
                Date = hour.Date,
                TaskId = hour.TaskId,
                Value = hour.Value,
                UserEmail = user?.Email,
                Comment = hour.Comment,
                CommentedAt = hour.CommentedAt
            };
        }

        throw new Exception("Kan ikke registrere time. Oppgaven er låst.");
    }

    public async Task<TimeEntryResponseDto> UpdateTimeEntry(CreateTimeEntryDto timeEntry, int userId)
    {
        var hour = await context.Hours.AsQueryable()
            .Filter(new TimeEntryQuerySearch
            {
                TaskId = timeEntry.TaskId,
                FromDateInclusive = timeEntry.Date.Date,
                ToDateInclusive = timeEntry.Date.Date,
                UserId = userId
            })
            .FirstOrDefaultAsync();

        var task = await context.Task
            .FirstOrDefaultAsync(t => t.Id == timeEntry.TaskId);

        if (!hour.Locked && !task.Locked)
        {
            hour.Value = timeEntry.Value;
            hour.TimeRegistered = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await UpdateComment(timeEntry.Comment, hour.Id);

            return new TimeEntryResponseDto
            {
                Id = hour.Id,
                User = hour.User,
                Date = hour.Date,
                TaskId = hour.TaskId,
                Value = hour.Value,
                UserEmail = hour.UserNavigation.Email,
                Comment = hour.Comment,
                CommentedAt = hour.CommentedAt,
            };
        }

        throw new Exception("Kan ikke oppdatere registrering. Oppgaven eller timen er låst.");
    }

    public async Task UpdateComment(string? comment, int hourId)
    {
        var hour = await context.Hours.FirstOrDefaultAsync(x => x.Id == hourId);
        hour.Comment = comment;
        hour.CommentedAt = comment != null ? DateTime.UtcNow : null;
        await context.SaveChangesAsync();
    }

    public async Task<List<EarnedOvertimeDto>> GetEarnedOvertime(OvertimeQueryFilter criteria)
    {
        var overtimeEntries = await context.EarnedOvertime.AsQueryable()
            .Filter(criteria)
            .Select(entry => new EarnedOvertimeDto
            {
                Date = entry.Date,
                Value = entry.Value,
                CompensationRate = entry.CompensationRate,
                UserId = entry.UserId
            })
            .ToListAsync();
        return overtimeEntries;
    }

    public async Task StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId)
    {
        context.EarnedOvertime.AddRange(overtimeEntries.Select(entry => new EarnedOvertime
        {
            Date = entry.Date,
            UserId = userId,
            Value = entry.Hours,
            CompensationRate = entry.CompensationRate
        }));
        await context.SaveChangesAsync();
    }

    public virtual async Task DeleteOvertimeOnDate(DateTime date, int userId)
    {
        var earnedOvertimeOnDate =
            context.EarnedOvertime.Where(ot => ot.Date.Date == date.Date && ot.UserId == userId);
        context.RemoveRange(earnedOvertimeOnDate);
        await context.SaveChangesAsync();
    }

    public Task<List<TimeEntryWithCustomerDto>> GetTimeEntriesWithCustomer(int userId, DateTime fromDate, DateTime toDate)
    {
        return (from hour in context.Hours
                where hour.Value > 0
                join task in context.Task on hour.TaskId equals task.Id
                join project in context.Project on task.Project equals project.Id
                join customer in context.Customer on project.Customer equals customer.Id
                where hour.User == userId
                      && hour.Date >= fromDate
                      && hour.Date <= toDate
                select new TimeEntryWithCustomerDto
                {
                    Date = hour.Date,
                    Value = hour.Value,
                    CustomerName = customer.Name,
                    TaskId = hour.TaskId
                }
        ).ToListAsync();
    }

    public async Task<IEnumerable<TimeEntryEmployeeResponseDto>> GetTimeEntriesForEmployees(MultipleTimeEntriesQuerySearch criteria)
    {
        var hours = await context.Hours.Include(h => h.Task).Include(h => h.UserNavigation).AsQueryable()
             .Filter(criteria)
             .Select(hour => new TimeEntryEmployeeResponseDto
             {
                 Id = hour.Id,
                 Date = hour.Date,
                 Value = hour.Value,
                 TaskId = hour.TaskId,
                 UserId = hour.User
             })
             .ToListAsync();

        return hours;
    }
}
