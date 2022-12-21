using System;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Holidays;
using AlvTime.Business.Interfaces;
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
        var userTasks = await _timeRegistrationStorage.GetTimeEntriesWithCustomer(user.Id, fromDate, toDate);
        var availableHoursWithoutVacation = GetUserAvailableHours(fromDate, toDate);

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
        decimal redDayHours = GetAmountOfRedDayHours(fromDate, toDate);

        var workingHours = 7.5m + (toDate - fromDate).Days * 7.5m;

        return workingHours - redDayHours;
    }


    private decimal GetAmountOfRedDayHours(DateTime fromDate, DateTime toDate)
    {
        var redDays = _redDaysService.GetRedDaysFromYears(fromDate.Year, toDate.Year)
            .Select(dateString => DateTime.Parse(dateString))
            .Where(date => date >= fromDate && date <= toDate);

        var weekendDays = DateUtils.GetWeekendDays(fromDate, toDate).Where(day => !redDays.Any(redDay => redDay == day));

        var amountOfRedDays = redDays.Count() + weekendDays.Count();

        return amountOfRedDays * 7.5m;
    }
}
