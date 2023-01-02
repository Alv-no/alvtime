using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Holidays;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Models;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using Microsoft.Extensions.Options;

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

    public async Task<InvoiceStatisticsDto> GetEmployeeInvoiceStatisticsByMonth(DateTime fromDate, DateTime toDate)
    {
        var user = await _userContext.GetCurrentUser();
        var invoiceRatePeriodStart = GetUserStartDateOrFromDate(fromDate, user);
        invoiceRatePeriodStart = new DateTime(invoiceRatePeriodStart.Year, invoiceRatePeriodStart.Month, 1);
        DateTime invoiceRatePeriodEnd = toDate.AddDays(0);

        if (toDate > DateTime.Now)
        {
            invoiceRatePeriodEnd = DateTime.Now;
        }

        var userTasks = await _timeRegistrationStorage.GetTimeEntriesWithCustomer(user.Id, invoiceRatePeriodStart, invoiceRatePeriodEnd);
        var taskDictionary = userTasks.GroupBy(GetTaskType)
                                      .ToDictionary(taskGroup => taskGroup.Key);

        var billableHours = GetMonthlyDataset(taskDictionary.ContainsKey(TaskType.BILLABLE) ? taskDictionary[TaskType.BILLABLE] : null, invoiceRatePeriodStart, invoiceRatePeriodEnd);
        var nonBillableHours = GetMonthlyDataset(taskDictionary.ContainsKey(TaskType.NON_BILLABLE) ? taskDictionary[TaskType.NON_BILLABLE] : null, invoiceRatePeriodStart, invoiceRatePeriodEnd);
        var vacationHours = GetMonthlyDataset(taskDictionary.ContainsKey(TaskType.VACATION) ? taskDictionary[TaskType.VACATION] : null, invoiceRatePeriodStart, invoiceRatePeriodEnd);

        return new InvoiceStatisticsDto
        {
            BillableHours = billableHours,
            NonBillableHours = nonBillableHours,
            VacationHours = vacationHours,
            InvoiceRate = GetInvoiceRateForArray(billableHours, vacationHours, invoiceRatePeriodStart, invoiceRatePeriodEnd),
            NonBillableInvoiceRate = GetInvoiceRateForArray(nonBillableHours, vacationHours, invoiceRatePeriodStart, invoiceRatePeriodEnd),
            Labels = GetMonthlyLabels(invoiceRatePeriodStart, billableHours.Count())
        };

    }

    private DateTime[] GetMonthlyLabels(DateTime fromDate, int monthCount)
    {
        var labels = new DateTime[monthCount];

        for (int i = 0; i < monthCount; i++)
        {
            labels[i] = fromDate.AddMonths(i);
        }

        return labels;
    }

    private decimal[] GetInvoiceRateForArray(decimal[] billableHours, decimal[] vacationHours, DateTime fromDate, DateTime toDate)
    {
        var result = new decimal[billableHours.Count()];

        for (int i = 0; i < billableHours.Count(); i++)
        {
            var toInterval = fromDate.AddMonths(i + 1).AddDays(-1);

            if (toInterval > toDate)
            {
                toInterval = toDate;
            }

            var availableHours = GetUserAvailableHours(fromDate.AddMonths(i), toInterval) - vacationHours[i];
            result[i] = billableHours[i] / (availableHours > 0 ? availableHours : 1);
        }

        return result;
    }

    private decimal[] GetMonthlyDataset(IEnumerable<TimeEntryWithCustomerDto> entries, DateTime fromDate, DateTime toDate)
    {
        var months = ((toDate.Year - fromDate.Year) * 12) + toDate.Month - fromDate.Month + 1;
        var result = new decimal[months];

        if (entries == null)
        {
            return result;
        }

        for (int i = 0; i < months; i++)
        {
            var period = fromDate.AddMonths(i);
            result[i] = entries.Where(entry => entry.Date.Month == period.Month && entry.Date.Year == period.Year).Sum(entry => entry.Value);
        }

        return result;
    }


    private TaskType GetTaskType(TimeEntryWithCustomerDto entry)
    {
        if (entry.TaskId == _timeEntryOptions.PaidHolidayTask || entry.TaskId == _timeEntryOptions.UnpaidHolidayTask)
        {
            return TaskType.VACATION;
        }

        if (entry.CustomerName.ToLower() == "alv")
        {
            return TaskType.NON_BILLABLE;
        }

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
        {
            return user.StartDate;
        }

        return fromDate;
    }
}
