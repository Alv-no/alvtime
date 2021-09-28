using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.EconomyData;
using FluentValidation;
using Microsoft.Data.SqlClient;

public class FlexhourStorage : IFlexhourStorage
{
    private const decimal HoursInRegularWorkday = 7.5M;
    private readonly ITimeEntryStorage _timeEntryStorage;
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
    private readonly int _flexTask;
    private readonly int _paidHolidayTask;
    private readonly int _unpaidHolidayTask;
    private readonly DateTime _startOfOvertimeSystem;
    private readonly ISalaryService _salaryService;

    public FlexhourStorage(ITimeEntryStorage timeEntryStorage, AlvTime_dbContext context, IOptionsMonitor<TimeEntryOptions> timeEntryOptions, ISalaryService salaryService)
    {
        _timeEntryStorage = timeEntryStorage;
        _context = context;
        _timeEntryOptions = timeEntryOptions;
        _flexTask = _timeEntryOptions.CurrentValue.FlexTask;
        _paidHolidayTask = _timeEntryOptions.CurrentValue.PaidHolidayTask;
        _unpaidHolidayTask = _timeEntryOptions.CurrentValue.UnpaidHolidayTask;
        _startOfOvertimeSystem = _timeEntryOptions.CurrentValue.StartOfOvertimeSystem;
        _salaryService = salaryService;
    }

    public AvailableHoursDto GetAvailableHours(int userId, DateTime userStartDate, DateTime endDate)
    {
        var dateToStartCalculation = userStartDate > _startOfOvertimeSystem ? userStartDate : _startOfOvertimeSystem;

        var overtimeEntries = GetOvertimeEntriesAfterOffTimeAndPayouts(dateToStartCalculation, endDate, userId);

        var sumFlexHours = overtimeEntries.Sum(o => o.Hours);
        var sumCompensatedHours = overtimeEntries.Sum(o => o.CompensationRate * o.Hours);

        return new AvailableHoursDto
        {
            AvailableHoursBeforeCompensation = sumFlexHours,
            AvailableHoursAfterCompensation = sumCompensatedHours,
            Entries = overtimeEntries
        };
    }

    public FlexedHoursDto GetFlexedHours(int userId)
    {
        var timeOffEntries = _context.Hours.Where(h => h.User == userId && h.TaskId == _flexTask);
        var timeOffSum = _context.Hours.Where(h => h.User == userId && h.TaskId == _flexTask).Sum(h => h.Value);

        return new FlexedHoursDto
        {
            TotalHours = timeOffSum,
            Entries = timeOffEntries.Select(entry => new GenericHourEntry
            {
                Date = entry.Date,
                Hours = entry.Value
            }).ToList()
        };
    }

    public PayoutsDto GetRegisteredPayouts(int userId)
    {
        var payouts = _context.PaidOvertime.Where(po => po.User == userId);
        var sumPayouts = payouts.Sum(po => po.HoursBeforeCompRate);

        return new PayoutsDto
        {
            TotalHours = sumPayouts,
            Entries = payouts.Select(po => new GenericPayoutEntry
            {
                Id = po.Id,
                Date = po.Date,
                HoursBeforeCompRate = po.HoursBeforeCompRate,
                HoursAfterCompRate = po.HoursAfterCompRate,
                Active = po.Date.Month >= DateTime.Now.Month && po.Date.Year == DateTime.Now.Year ? true : false
            }).ToList()
        };
    }

    private List<OvertimeEntry> GetOvertimeEntriesAfterOffTimeAndPayouts(DateTime startDate, DateTime endDate, int userId)
    {
        List<DateEntry> entriesByDate = GetTimeEntries(startDate, endDate, userId);
        var registeredPayouts = GetRegisteredPayouts(userId);

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, endDate, userId);
        CompensateForRegisteredPayouts(overtimeEntries, registeredPayouts);

        return overtimeEntries;
    }

    private List<DateEntry> GetTimeEntries(DateTime startDate, DateTime endDate, int userId)
    {
        var entriesByDate = _timeEntryStorage.GetDateEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = startDate,
            ToDateInclusive = endDate
        }).ToList();

        var daysNotRecorded = GetDaysInPeriod(startDate, endDate).Where(day => !entriesByDate.Select(e => e.Date).Contains(day));
        foreach (var day in daysNotRecorded)
        {
            entriesByDate.Add(new DateEntry
            {
                Date = day,
                Entries = new[] { new Entry { Value = 0 } }
            });
        }

        return entriesByDate;
    }

    private List<OvertimeEntry> GetOvertimeEntries(IEnumerable<DateEntry> entriesByDate, DateTime startDate, DateTime endDate)
    {
        var overtimeEntries = new List<OvertimeEntry>();
        var years = entriesByDate.Select(x => x.Date.Year).Distinct();
        var allRedDays = new List<DateTime>();

        foreach (var year in years)
        {
            allRedDays.AddRange(new RedDays(year).Dates);
        }

        foreach (var currentDate in GetDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day != null && WorkedOnRedDay(day, allRedDays))
            {
                overtimeEntries.AddRange(CreateOvertimeEntries(day, isRedDay: true));
            }
            else if (day != null && day.GetWorkingHours() > HoursInRegularWorkday)
            {
                overtimeEntries.AddRange(CreateOvertimeEntries(day, isRedDay: false));
            }
        }
        return overtimeEntries;
    }


    private IEnumerable<OvertimeEntry> CreateOvertimeEntries(DateEntry day, bool isRedDay)
    {
        var overtimeHours = isRedDay ? day.GetWorkingHours() : day.GetWorkingHours() - HoursInRegularWorkday;

        foreach (var entry in day.Entries.OrderBy(task => task.CompensationRate))
        {
            if (overtimeHours <= 0)
            {
                break;
            }

            if (isRedDay && (entry.TaskId == _paidHolidayTask || entry.TaskId == _unpaidHolidayTask))
            {
                continue;
            }

            OvertimeEntry overtimeEntry = new OvertimeEntry
            {
                Hours = Math.Min(overtimeHours, entry.Value),
                CompensationRate = entry.CompensationRate,
                Date = day.Date,
                TaskId = entry.TaskId
            };

            overtimeHours -= overtimeEntry.Hours;

            yield return overtimeEntry;
        }
    }


    private bool WorkedOnRedDay(DateEntry day, List<DateTime> redDays)
    {
        if ((IsWeekend(day) ||
            redDays.Contains(day.Date)) &&
            day.GetWorkingHours() > 0)
        {
            return true;
        }
        return false;
    }

    private bool IsWeekend(DateEntry entry)
    {
        return entry.Date.DayOfWeek == DayOfWeek.Saturday || entry.Date.DayOfWeek == DayOfWeek.Sunday;
    }

    private void CompensateForOffTime(List<OvertimeEntry> overtimeEntries, IEnumerable<DateEntry> entriesByDate, DateTime startDate, DateTime endDate, int userId)
    {
        foreach (var currentWorkDay in GetDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentWorkDay);
            var entriesWithTimeOff = day.Entries.Where(e => e.TaskId == _flexTask);

            if (day != null && entriesWithTimeOff.Any())
            {
                var hoursOffThisDay = entriesWithTimeOff.Sum(e => e.Value);

                var orderedOverTime = overtimeEntries
                    .Where(o => o.Date < currentWorkDay)
                    .GroupBy(
                        hours => hours.CompensationRate,
                        hours => hours,
                        (cr, hours) => new
                        {
                            CompensationRate = cr,
                            Hours = hours.Sum(h => h.Hours)
                        })
                    .OrderByDescending(h => h.CompensationRate);

                foreach (var entry in orderedOverTime)
                {
                    if (hoursOffThisDay <= 0)
                    {
                        break;
                    }

                    OvertimeEntry overtimeEntry = new OvertimeEntry
                    {
                        Hours = -Math.Min(hoursOffThisDay, entry.Hours),
                        CompensationRate = entry.CompensationRate,
                        Date = day.Date
                    };

                    overtimeEntries.Add(overtimeEntry);
                    hoursOffThisDay += overtimeEntry.Hours;
                }
            }
        }
    }

    private void CompensateForRegisteredPayouts(List<OvertimeEntry> overtimeEntries, PayoutsDto registeredPayouts)
    {
        var payoutEntriesGroupedByDate = registeredPayouts.Entries.GroupBy(e => e.Date).OrderBy(g => g.Key);

        foreach (var payoutEntryGroup in payoutEntriesGroupedByDate)
        {
            var payoutDate = payoutEntryGroup.Key;
            var registeredPayoutsTotal = payoutEntryGroup.Sum(e => e.HoursBeforeCompRate);

            var orderedOverTime = overtimeEntries.Where(e => e.Date <= payoutDate).GroupBy(
                hours => hours.CompensationRate,
                hours => hours,
                (cr, hours) => new
                {
                    CompensationRate = cr,
                    Hours = hours.Sum(h => h.Hours)
                })
                .OrderBy(h => h.CompensationRate);

            foreach (var entry in orderedOverTime)
            {
                if (registeredPayoutsTotal <= 0)
                {
                    break;
                }

                OvertimeEntry overtimeEntry = new OvertimeEntry
                {
                    Hours = -Math.Min(registeredPayoutsTotal, entry.Hours),
                    CompensationRate = entry.CompensationRate,
                    Date = payoutDate
                };

                overtimeEntries.Add(overtimeEntry);
                registeredPayoutsTotal += overtimeEntry.Hours;
            }
        }
    }

    private IEnumerable<DateTime> GetDaysInPeriod(DateTime from, DateTime to)
    {
        for (DateTime currentDate = from; currentDate <= to; currentDate += TimeSpan.FromDays(1))
        {
            yield return currentDate;
        }
    }

    public PaidOvertimeEntry RegisterPaidOvertime(GenericHourEntry request, int userId)
    {
        var user = _context.User.SingleOrDefault(u => u.Id == userId);
        var userStartDate = user.StartDate;

        var dateToStartCalculation = userStartDate > _startOfOvertimeSystem ? userStartDate : _startOfOvertimeSystem;
        var overtimeEntries = GetOvertimeEntriesAfterOffTimeAndPayouts(dateToStartCalculation, request.Date, userId);
        var availableForPayout = overtimeEntries.Sum(ot => ot.Hours);
        var hoursAfterCompRate = GetHoursAfterCompRate(overtimeEntries, request.Hours);
      
        if (request.Hours <= availableForPayout)
        {
            using (var alvDbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    PaidOvertime paidOvertime = new PaidOvertime
                    {
                        Date = request.Date,
                        User = userId,
                        HoursBeforeCompRate = request.Hours,
                        HoursAfterCompRate = hoursAfterCompRate
                    };

                    _context.PaidOvertime.Add(paidOvertime);
                    _context.SaveChanges();
                    
                    var paidOvertimeSalary = RegisterOvertimePayout(overtimeEntries, userId, request, paidOvertime.Id);

                    alvDbContextTransaction.Commit();

                    return new PaidOvertimeEntry()
                    {
                        Id = paidOvertime.Id,
                        UserId = paidOvertime.User,
                        Date = paidOvertime.Date,
                        HoursBeforeCompensation = paidOvertime.HoursBeforeCompRate,
                        HoursAfterCompensation = paidOvertime.HoursAfterCompRate
                    };
                }
                catch (Exception e)
                {
                    alvDbContextTransaction.Rollback();
                    throw new ValidationException($"Could not register overtime for user {userId}");
                }
            }
            

        }

        throw new ValidationException("Not enough available hours");
    }



    public PaidOvertimeEntry CancelPayout(int userId, int id)
    {
        var paidOvertime = _context.PaidOvertime.FirstOrDefault(po => po.Id == id && po.User == userId);

        if (!CanBeDeleted(paidOvertime))
        {
            throw new ValidationException("Selected payout must be latest ordered payout");
        }

        if (paidOvertime != null && paidOvertime.Date.Month >= DateTime.Now.Month && paidOvertime.Date.Year == DateTime.Now.Year)
        {
            using (var alvDbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.PaidOvertime.Remove(paidOvertime);
                    _context.SaveChanges();

                    _salaryService.DeleteOvertimePayout(userId, paidOvertime.Id);
                    alvDbContextTransaction.Commit();

                    return new PaidOvertimeEntry
                    {
                        Date = paidOvertime.Date,
                        Id = paidOvertime.Id,
                        UserId = paidOvertime.User,
                        HoursBeforeCompensation = paidOvertime.HoursBeforeCompRate,
                        HoursAfterCompensation = paidOvertime.HoursAfterCompRate
                    };
                }
                catch (Exception e)
                {
                    alvDbContextTransaction.Rollback();
                    throw new ValidationException($"Unable to delete paid overtime entry with Id {id}");
                }

            }
        }

        throw new ValidationException("Selected payout is not active");
    }

    private bool CanBeDeleted(PaidOvertime payout)
    {
        var allActivePayouts = _context.PaidOvertime
            .Where(p => p.Date.Month >= DateTime.Now.Month &&
                                    p.Date.Year == DateTime.Now.Year).ToList();

        if (!allActivePayouts.Any())
        {
            throw new ValidationException("There are no active payouts");
        }

        var latestId = allActivePayouts.OrderBy(p => p.Id).Last().Id;

        if (payout.Id < latestId)
        {
            return false;
        }

        return true;
    }

    private decimal GetHoursAfterCompRate(List<OvertimeEntry> overtimeEntries, decimal orderedHours)
    {
        var totalPayout = 0M;

        var orderedOverTime = overtimeEntries.GroupBy(
            hours => hours.CompensationRate,
            hours => hours,
            (cr, hours) => new
            {
                CompensationRate = cr,
                Hours = hours.Sum(h => h.Hours)
            })
            .OrderBy(h => h.CompensationRate);

        foreach (var entry in orderedOverTime)
        {
            if (orderedHours <= 0)
            {
                break;
            }

            var hoursBeforeCompensation = Math.Min(orderedHours, entry.Hours);
            totalPayout += hoursBeforeCompensation * entry.CompensationRate;
            orderedHours -= hoursBeforeCompensation;
        }

        return totalPayout;
    }
    
    private Dictionary<decimal, List<OvertimeEntryWithSalary>> GetOvertimeEntriesWithSalaryGroupedByCompensationRate(List<EmployeeSalaryDto> salaryData, List<OvertimeEntry> overtimeEntries)
    {
        var overtimeWithSalaryDictionary = new Dictionary<decimal, List<OvertimeEntryWithSalary>>();
        var overtimeEntriesGroupedByCompRate = overtimeEntries.GroupBy(x => x.CompensationRate);

        foreach (var overtimeEntryGroupForGivenCompRate in overtimeEntriesGroupedByCompRate)
        {
            var overtimeEntriesForGivenCompRate = new List<OvertimeEntryWithSalary>();
            foreach (var overtimeEntry in overtimeEntryGroupForGivenCompRate)
            {
                var salaryForOvertimeEntry = salaryData.FirstOrDefault(employeeSalary => overtimeEntry.Date >= employeeSalary.FromDate && (employeeSalary.ToDate == null || overtimeEntry.Date < employeeSalary.ToDate)).HourlySalary;
                overtimeEntriesForGivenCompRate.Add(new OvertimeEntryWithSalary(salaryForOvertimeEntry, overtimeEntry));
            }

            overtimeWithSalaryDictionary.Add(overtimeEntryGroupForGivenCompRate.Key, overtimeEntriesForGivenCompRate);
        }
        return overtimeWithSalaryDictionary;
    }
    
    private List<OvertimeEntryWithSalary> GetOvertimeEntriesForGivenCompRateForPayoutCalculation(List<OvertimeEntryWithSalary> overtimeEntries)
    {
        var negativeOvertimeCounterToFilterOutTimeOffAndPayout = overtimeEntries
            .Where(x => x.OvertimeEntry.Hours < 0M)
            .Sum(x => x.OvertimeEntry.Hours);

        if (negativeOvertimeCounterToFilterOutTimeOffAndPayout >= 0M)
        {
            return overtimeEntries;
        }
        
        var overtimeEntriesOrderedByDate = overtimeEntries.OrderBy(x => x.OvertimeEntry.Date).ToList();
        var overtimeEntriesForPayoutCalculation = new List<OvertimeEntryWithSalary>();

        for (int overtimeEntryIndex = 0; overtimeEntryIndex < overtimeEntries.Count; overtimeEntryIndex++)
        {
            if (overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours < 0M)
            {
                continue;
            }

            if (negativeOvertimeCounterToFilterOutTimeOffAndPayout == 0M)
            {
                overtimeEntriesForPayoutCalculation.Add(overtimeEntriesOrderedByDate[overtimeEntryIndex]);
            }
            else if (negativeOvertimeCounterToFilterOutTimeOffAndPayout + overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours <= 0M)
            {
                
                    negativeOvertimeCounterToFilterOutTimeOffAndPayout += overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours;
                
            }
            else if (negativeOvertimeCounterToFilterOutTimeOffAndPayout + overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours > 0M)
            {
                overtimeEntriesForPayoutCalculation.Add(new OvertimeEntryWithSalary(overtimeEntriesOrderedByDate[overtimeEntryIndex].SalaryPrHour, new OvertimeEntry
                {
                    CompensationRate = overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.CompensationRate,
                    Date = overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Date,
                    Hours = overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours + negativeOvertimeCounterToFilterOutTimeOffAndPayout,
                    Salary = overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.Salary,
                    TaskId = overtimeEntriesOrderedByDate[overtimeEntryIndex].OvertimeEntry.TaskId
                }));
                negativeOvertimeCounterToFilterOutTimeOffAndPayout+=(0M-negativeOvertimeCounterToFilterOutTimeOffAndPayout);
            }
        }
        return overtimeEntriesForPayoutCalculation;
    }

    private decimal CalculatePayoutForOvertimeEntries(Dictionary<decimal, List<OvertimeEntryWithSalary>> compRateAndOvertimeEntriesWithSalary, decimal requestedHoursForPayout)
    {
        var hoursForPayoutCounter = 0M;
        var overtimeEntriesForPayoutCalculation = new List<OvertimeEntryWithSalary>();
        var overtimeEntriesWithSalaryOrderedByCompRate = compRateAndOvertimeEntriesWithSalary.OrderBy(compRateWithOvertimeEntries => compRateWithOvertimeEntries.Key);
        
        for (int overtimeEntriesWithSalaryIndex = 0; overtimeEntriesWithSalaryIndex < overtimeEntriesWithSalaryOrderedByCompRate.Count(); overtimeEntriesWithSalaryIndex++)
        {
            if (hoursForPayoutCounter == requestedHoursForPayout)
            {
                break;
            }

            var overtimeEntriesForCurrentComprate = overtimeEntriesWithSalaryOrderedByCompRate
                .ElementAt(overtimeEntriesWithSalaryIndex).Value
                .OrderBy(overtimeEntryWithSalary => overtimeEntryWithSalary.OvertimeEntry.Date).ToList();

            //ser at visual studio foreslå en foreach-loop, men her er rekkefølgen viktig => for-loop må beholdes
            for (var overtimeEntryIndex = 0; overtimeEntryIndex < overtimeEntriesForCurrentComprate.Count; overtimeEntryIndex++)
            {
                if (hoursForPayoutCounter == requestedHoursForPayout)
                {
                    break;
                }

                if (overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.Hours + hoursForPayoutCounter > requestedHoursForPayout)
                {
                    overtimeEntriesForPayoutCalculation.Add(new OvertimeEntryWithSalary(
                        overtimeEntriesForCurrentComprate[overtimeEntryIndex].SalaryPrHour,
                        new OvertimeEntry
                        {
                            CompensationRate = overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.CompensationRate,
                            Date = overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.Date,
                            Hours = requestedHoursForPayout - hoursForPayoutCounter,
                            Salary = overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.Salary,
                            TaskId = overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.TaskId
                        }));
                    hoursForPayoutCounter += (requestedHoursForPayout - hoursForPayoutCounter);
                }

                else if (overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.Hours + hoursForPayoutCounter <= requestedHoursForPayout)
                {
                    hoursForPayoutCounter += overtimeEntriesForCurrentComprate[overtimeEntryIndex].OvertimeEntry.Hours;
                    overtimeEntriesForPayoutCalculation.Add(overtimeEntriesForCurrentComprate[overtimeEntryIndex]);
                }
            }
        }

        return overtimeEntriesForPayoutCalculation
            .Sum(overtimeEntry => 
                overtimeEntry.OvertimeEntry.CompensationRate * overtimeEntry.OvertimeEntry.Hours * overtimeEntry.SalaryPrHour);
    }
    
    public decimal RegisterOvertimePayout(List<OvertimeEntry> overtimeEntries, int userId, GenericHourEntry requestedPayout, int paidOvertimeId)
    {
        var salaryData = _salaryService.GetEmployeeSalaryData(userId).OrderBy(x => x.FromDate).ToList();
        var overtimeEntriesByCompRateWithSalary = GetOvertimeEntriesWithSalaryGroupedByCompensationRate(salaryData, overtimeEntries);

        foreach ( var (compRate, overtimeEntriesWithSalary) in overtimeEntriesByCompRateWithSalary)
        {
            overtimeEntriesByCompRateWithSalary[compRate] = GetOvertimeEntriesForGivenCompRateForPayoutCalculation(overtimeEntriesWithSalary);
        }

        var salaryPayout = CalculatePayoutForOvertimeEntries(overtimeEntriesByCompRateWithSalary, requestedPayout.Hours);

        var overtimePayout = _salaryService.SaveOvertimePayout(
            new RegisterOvertimePayout(
                userId,
                requestedPayout.Date,
                salaryPayout,
                paidOvertimeId)
            );
        return overtimePayout.TotalPayout;
    }
}