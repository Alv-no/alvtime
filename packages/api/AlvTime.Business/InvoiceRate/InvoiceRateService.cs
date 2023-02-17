using AlvTime.Business.Holidays;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Models;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AlvTime.Business.InvoiceRate.InvoiceStatisticsDto;

namespace AlvTime.Business.InvoiceRate;

public class InvoiceRateService
{
    private readonly ITimeRegistrationStorage _timeRegistrationStorage;
    private readonly IRedDaysService _redDaysService;
    private readonly TimeEntryOptions _timeEntryOptions;
    private readonly IUserContext _userContext;

    public InvoiceRateService(ITimeRegistrationStorage timeRegistrationStorage,
        IRedDaysService redDaysService,
        IOptionsMonitor<TimeEntryOptions> optionsMonitor,
        IUserContext userContext
    )
    {
        _timeRegistrationStorage = timeRegistrationStorage;
        _redDaysService = redDaysService;
        _timeEntryOptions = optionsMonitor.CurrentValue;
        _userContext = userContext;
    }

    public async Task<decimal> GetEmployeeInvoiceRateForPeriod(DateTime fromDate, DateTime toDate)
    {
        var user = await _userContext.GetCurrentUser();
        var adjustedFromDate = GetUserStartDateOrFromDate(fromDate, user);
        var userTasks = await _timeRegistrationStorage.GetTimeEntriesWithCustomer(user.Id, adjustedFromDate, toDate);
        var availableHoursWithoutVacation = GetUserAvailableHours(adjustedFromDate, toDate);

        var taskDictionary = userTasks.GroupBy(GetTaskType)
                                      .ToDictionary(taskGroup => taskGroup.Key);

        decimal billableHours = taskDictionary.ContainsKey(TaskType.BILLABLE) ? taskDictionary[TaskType.BILLABLE].Sum(task => task.Value) : 0;
        decimal vacationHours = taskDictionary.ContainsKey(TaskType.VACATION) ? taskDictionary[TaskType.VACATION].Sum(task => task.Value) : 0;

        var availableHours = availableHoursWithoutVacation - vacationHours;

        if (availableHours <= 0)
        {
            return billableHours / 1;
        }

        return billableHours / availableHours;
    }

    public async Task<InvoiceStatisticsDto> GetEmployeeInvoiceStatisticsByPeriod(DateTime fromDate, DateTime toDate, InvoicePeriods invoicePeriod, ExtendPeriod extendPeriod, bool includeZeroPeriods)
    {
        var user = await _userContext.GetCurrentUser();
        var invoicePeriodStart = GetInvoicePeriodStart(fromDate, invoicePeriod, extendPeriod);
        invoicePeriodStart = (invoicePeriodStart >= user.StartDate) ? invoicePeriodStart : user.StartDate.Date;
        var invoicePeriodEnd = GetInvoicePeriodEnd(toDate, invoicePeriod, extendPeriod);
        invoicePeriodEnd = (invoicePeriodEnd <= DateTime.Now) ? invoicePeriodEnd : DateTime.Now.Date.AddSeconds(86399);

        var userTasks = await _timeRegistrationStorage.GetTimeEntriesWithCustomer(user.Id, invoicePeriodStart, invoicePeriodEnd);
        var taskPeriodGrouping = GroupTasksByInvoicePeriod(userTasks, invoicePeriod);

        var billableHours = new List<decimal>();
        var nonBillableHours = new List<decimal>();
        var vacationHours = new List<decimal>();
        var invoiceRates = new List<decimal>();
        var nonBillableInvoiceRate = new List<decimal>();
        var starts = new List<DateTime>();
        var ends = new List<DateTime>();

        var previousPeriodEnd = invoicePeriodStart.AddSeconds(-1);
        foreach (var grouping in taskPeriodGrouping)
        {
            var periodStart = (grouping.Key.periodStart < invoicePeriodStart) ? invoicePeriodStart : grouping.Key.periodStart;
            var periodEnd = (grouping.Key.periodEnd > invoicePeriodEnd) ? invoicePeriodEnd : grouping.Key.periodEnd;

            while (includeZeroPeriods && (periodStart - previousPeriodEnd).TotalSeconds != 1)
            {
                var start = previousPeriodEnd.AddSeconds(1);
                var end = GetInvoicePeriodEnd(start, invoicePeriod, ExtendPeriod.End);

                billableHours.Add(0);
                nonBillableHours.Add(0);
                vacationHours.Add(0);
                invoiceRates.Add(0);
                nonBillableInvoiceRate.Add(0);
                starts.Add(start);
                ends.Add(end);

                previousPeriodEnd = end;
            }

            var currentBillableHours = grouping.Where(timeEntry => GetTaskType(timeEntry) == TaskType.BILLABLE).Sum(timeEntry => timeEntry.Value);
            var currentNonBillableHours = grouping.Where(timeEntry => GetTaskType(timeEntry) == TaskType.NON_BILLABLE).Sum(timeEntry => timeEntry.Value);
            var currentVacationHours = grouping.Where(timeEntry => GetTaskType(timeEntry) == TaskType.VACATION).Sum(timeEntry => timeEntry.Value);

            billableHours.Add(currentBillableHours);
            nonBillableHours.Add(currentNonBillableHours);
            vacationHours.Add(currentVacationHours);
            invoiceRates.Add(GetInvoiceRateForPeriod(currentBillableHours, currentVacationHours, periodStart, periodEnd));
            nonBillableInvoiceRate.Add(GetInvoiceRateForPeriod(currentNonBillableHours, currentVacationHours, periodStart, periodEnd));
            starts.Add(grouping.Key.periodStart);
            ends.Add(periodEnd);

            previousPeriodEnd = periodEnd;
        }


        return new InvoiceStatisticsDto
        {
            Start = starts.ToArray(),
            End = ends.ToArray(),
            BillableHours = billableHours.ToArray(),
            InvoiceRate = invoiceRates.ToArray(),
            NonBillableHours = nonBillableHours.ToArray(),
            NonBillableInvoiceRate = nonBillableInvoiceRate.ToArray(),
            VacationHours = vacationHours.ToArray()
        };
    }

    private IEnumerable<IGrouping<(DateTime periodStart, DateTime periodEnd), TimeEntryWithCustomerDto>> GroupTasksByInvoicePeriod(List<TimeEntryWithCustomerDto> userTasks, InvoicePeriods invoicePeriod)
    {
        return invoicePeriod switch
        {
            InvoicePeriods.Daily => userTasks.GroupBy(x => (x.Date.Date, new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, 23, 59, 59))),
            InvoicePeriods.Weekly => userTasks.GroupBy(x => (GetStartOfWeek(x.Date).Date, GetEndOfWeek(x.Date))),
            InvoicePeriods.Monthly => userTasks.GroupBy(x => (new DateTime(x.Date.Year, x.Date.Month, 1), new DateTime(x.Date.Year, x.Date.Month, DateTime.DaysInMonth(x.Date.Year, x.Date.Month), 23, 59, 59))),
            InvoicePeriods.Annualy => userTasks.GroupBy(x => (new DateTime(x.Date.Year, 1, 1), new DateTime(x.Date.Year, 12, 31, 23, 59, 59))),
            _ => throw new NotImplementedException()
        };
    }

    private DateTime GetInvoicePeriodStart(DateTime fromDate, InvoicePeriods invoicePeriod, ExtendPeriod extendperiod)
    {
        if (!extendperiod.HasFlag(ExtendPeriod.Start))
            return new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);

        return invoicePeriod switch
        {
            InvoicePeriods.Daily => new DateTime(fromDate.Year, fromDate.Month, fromDate.Day),
            InvoicePeriods.Weekly => GetStartOfWeek(fromDate).Date,
            InvoicePeriods.Monthly => new DateTime(fromDate.Year, fromDate.Month, 1),
            InvoicePeriods.Annualy => new DateTime(fromDate.Year, 1, 1),
            _ => throw new NotImplementedException()
        };
    }


    private DateTime GetInvoicePeriodEnd(DateTime toDate, InvoicePeriods invoicePeriod, ExtendPeriod extendperiod)
    {
        if (!extendperiod.HasFlag(ExtendPeriod.End))
            return new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);

        return invoicePeriod switch
        {
            InvoicePeriods.Daily => new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59),
            InvoicePeriods.Weekly => GetEndOfWeek(toDate),
            InvoicePeriods.Monthly => new DateTime(toDate.Year, toDate.Month, DateTime.DaysInMonth(toDate.Year, toDate.Month), 23, 59, 59),
            InvoicePeriods.Annualy => new DateTime(toDate.Year, 12, 31, 23, 59, 59),
            _ => throw new NotImplementedException()
        };
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-6) : date.AddDays(1 - (int)date.DayOfWeek).Date;
    }

    private static DateTime GetEndOfWeek(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Sunday ? date.AddSeconds(86399) : date.Date.AddDays(7 - (int)date.DayOfWeek).Date.AddSeconds(86399);
    }

    private decimal GetInvoiceRateForPeriod(decimal billableHours, decimal vacationHours, DateTime fromDate, DateTime toDate)
    {
        var availableHours = GetUserAvailableHours(fromDate, toDate) - vacationHours;
        return billableHours / (availableHours > 0 ? availableHours : 1);
    }

    private TaskType GetTaskType(TimeEntryWithCustomerDto entry)
    {
        if (entry.TaskId == _timeEntryOptions.PaidHolidayTask || entry.TaskId == _timeEntryOptions.UnpaidHolidayTask)
            return TaskType.VACATION;
        if (entry.CustomerName.ToLower() == "alv")
            return TaskType.NON_BILLABLE;
        return TaskType.BILLABLE;
    }

    private decimal GetUserAvailableHours(DateTime fromDate, DateTime toDate)
    {
        decimal redDayHours = GetNonWorkingDays(fromDate, toDate).Count() * 7.5m;
        var workingHours = 7.5m + (toDate - fromDate).Days * 7.5m;
        return workingHours - redDayHours;
    }


    private IEnumerable<DateTime> GetNonWorkingDays(DateTime fromDate, DateTime toDate)
    {
        var redDays = _redDaysService.GetRedDaysFromYears(fromDate.Year, toDate.Year)
            .Select(dateString => DateTime.Parse(dateString))
            .Where(date => date >= fromDate && date <= toDate);
        var weekendDays = DateUtils.GetWeekendDays(fromDate, toDate).Where(day => !redDays.Any(redDay => redDay == day));
        return weekendDays.Concat(redDays);
    }

    private DateTime GetUserStartDateOrFromDate(DateTime fromDate, User user)
    {
        if (fromDate < user.StartDate)
            return user.StartDate;
        return fromDate;
    }
}
