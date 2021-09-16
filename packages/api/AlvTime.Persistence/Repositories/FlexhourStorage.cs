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

            return new PaidOvertimeEntry()
            {
                UserId = paidOvertime.Id,
                Date = paidOvertime.Date,
                HoursBeforeCompensation = paidOvertime.HoursBeforeCompRate,
                HoursAfterCompensation = paidOvertime.HoursAfterCompRate,
                CompensationSalary = paidOvertimeSalary
            };
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
            _context.PaidOvertime.Remove(paidOvertime);
            _context.SaveChanges();

            _salaryService.DeleteOvertimePayout(userId, paidOvertime.Id);

            return new PaidOvertimeEntry
            {
                Date = paidOvertime.Date,
                Id = paidOvertime.Id,
                UserId = paidOvertime.User,
                HoursBeforeCompensation = paidOvertime.HoursBeforeCompRate,
                HoursAfterCompensation = paidOvertime.HoursAfterCompRate
            };
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
    
    private List<List<OvertimeEntryWithSalary>> GetOvertimeEntriesWithSalaryGroupedByCompensationRate(List<EmployeeSalary> salaryData, List<OvertimeEntry> overtimeEntries)
    {
        var overtimeEntriesGruopedBySalary = new List<List<OvertimeEntryWithSalary>>();

        var overtimeEntriesGroupedByCompRate =
            overtimeEntries.GroupBy(x => x.CompensationRate);

        foreach (var overtimeEntryGroupForGivenCompRate in overtimeEntriesGroupedByCompRate)
        {
            var overtimeEntriesForGivenCompRate = new List<OvertimeEntryWithSalary>();
            foreach (var overtimeEntry in overtimeEntryGroupForGivenCompRate)
            {
                var salaryForOvertimeEntry = salaryData.FirstOrDefault(x => overtimeEntry.Date >= x.FromDate && (x.ToDate == null || overtimeEntry.Date < x.ToDate)).HourlySalary;
                overtimeEntriesForGivenCompRate.Add(new OvertimeEntryWithSalary(salaryForOvertimeEntry, overtimeEntry));
            }
            overtimeEntriesGruopedBySalary.Add(overtimeEntriesForGivenCompRate);
        }

        return overtimeEntriesGruopedBySalary;
    }

    private List<List<OvertimeEntryWithSalary>> GetOvertimeEntriesFilteredForPayoutsAndTimeOff(List<List<OvertimeEntryWithSalary>> overtimeEntriesWithSalaryGroupedByCompRate)
    {
        var filteredOvertimeEntries = 
            overtimeEntriesWithSalaryGroupedByCompRate
            .Select(overtimeEntriesWithSalaryList => GetOvertimeEntriesForGivenCompRateForPayoutCalculation(overtimeEntriesWithSalaryList))
            .ToList();

        if (filteredOvertimeEntries.Any(x => x.Item2 < 0M))
        {
            return GetPositiveOvertimeEntriesWithSalaryCompensateForOffTimeAndPayOutOverDifferentCompensationRates(
                filteredOvertimeEntries.Select(x => x.Item1).ToList()); 
        }

        return filteredOvertimeEntries.Select(x => x.Item1).ToList();
    }

    private List<List<OvertimeEntryWithSalary>> GetPositiveOvertimeEntriesWithSalaryCompensateForOffTimeAndPayOutOverDifferentCompensationRates(List<List<OvertimeEntryWithSalary>> overtimeEntries)
    {

        var negativeOVertime =
            overtimeEntries.Sum(x => x.Where(x => x.OvertimeEntry.Hours < 0M).Sum(x => x.OvertimeEntry.Hours));
        if (negativeOVertime >= 0M)
        {
            return overtimeEntries;
        }
        var returListe = new List<List<OvertimeEntryWithSalary>>();
        var negativeOvertimeCounter = negativeOVertime;
        var indexListe = new List<(int, decimal)>();
        foreach (var oeWithSalary in overtimeEntries)
        {
            var overtimeEntryWithSalary = oeWithSalary.FirstOrDefault();

            indexListe.Add((overtimeEntries.IndexOf(oeWithSalary), overtimeEntryWithSalary.OvertimeEntry.CompensationRate));
        }
        var indexlisteForIterasjoner = indexListe.OrderByDescending(x => x.Item2).ToList();

        for (int indeksen = 0; indeksen < indexListe.Count; indeksen++)
        {
            var indeksForUthentingAvRiktigCR = indexlisteForIterasjoner[indeksen].Item1;
            var oeForComprateListe = overtimeEntries[indeksForUthentingAvRiktigCR].OrderBy(x => x.OvertimeEntry.Date).ToList();
            var placeHolderListe = new List<OvertimeEntryWithSalary>();
            for (int oeIndeksen = 0; oeIndeksen < oeForComprateListe.Count; oeIndeksen++)
            {
                if (oeForComprateListe[oeIndeksen].OvertimeEntry.Hours < 0M)
                {
                    continue;
                }

                if (negativeOvertimeCounter + oeForComprateListe[oeIndeksen].OvertimeEntry.Hours == 0M || oeForComprateListe[oeIndeksen].OvertimeEntry.Hours == 0M)
                {
                    if (negativeOvertimeCounter != 0M)
                    {
                        negativeOvertimeCounter += oeForComprateListe[oeIndeksen].OvertimeEntry.Hours;
                    }
                    else
                    {
                        placeHolderListe.Add(new OvertimeEntryWithSalary(oeForComprateListe[oeIndeksen].SalaryPrHour, oeForComprateListe[oeIndeksen].OvertimeEntry));
                    }
                }
                else if (negativeOvertimeCounter + oeForComprateListe[oeIndeksen].OvertimeEntry.Hours > 0M)
                {
                    placeHolderListe.Add(new OvertimeEntryWithSalary(oeForComprateListe[oeIndeksen].SalaryPrHour, new OvertimeEntry
                    {
                        CompensationRate = oeForComprateListe[oeIndeksen].OvertimeEntry.CompensationRate,
                        Date = oeForComprateListe[oeIndeksen].OvertimeEntry.Date,
                        Hours = negativeOvertimeCounter + oeForComprateListe[oeIndeksen].OvertimeEntry.Hours,
                        Salary = oeForComprateListe[oeIndeksen].OvertimeEntry.Salary,
                        TaskId = oeForComprateListe[oeIndeksen].OvertimeEntry.TaskId
                    }));
                    negativeOvertimeCounter = 0M;
                }
                else if (negativeOvertimeCounter + oeForComprateListe[oeIndeksen].OvertimeEntry.Hours < 0M)
                {
                    negativeOvertimeCounter += oeForComprateListe[oeIndeksen].OvertimeEntry.Hours;
                }
            }
            returListe.Add(placeHolderListe);
        }

        return returListe;
    }

    private (List<OvertimeEntryWithSalary> filteredOvertimeEntries, decimal restHours) GetOvertimeEntriesForGivenCompRateForPayoutCalculation(List<OvertimeEntryWithSalary> overtimeEntries)
    {
        var negativeOvertimeCounterToFilterOutTimeOffAndPayout = overtimeEntries.Where(x => x.OvertimeEntry.Hours < 0M).Sum(x => x.OvertimeEntry.Hours);

        if (negativeOvertimeCounterToFilterOutTimeOffAndPayout >= 0M)
        {
            return (overtimeEntries, negativeOvertimeCounterToFilterOutTimeOffAndPayout);
        }
        
        var orderedOvertimeEntries = overtimeEntries.OrderBy(x => x.OvertimeEntry.Date).ToList();
        var overtimeEntriesForPayoutCalculation = new List<OvertimeEntryWithSalary>();
        for (int overtimeEntryIndex = 0; overtimeEntryIndex < overtimeEntries.Count; overtimeEntryIndex++)
        {
            if (orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Hours < 0M)
            {
                continue;
            }

            if (negativeOvertimeCounterToFilterOutTimeOffAndPayout == 0M)
            {
                overtimeEntriesForPayoutCalculation.Add(orderedOvertimeEntries[overtimeEntryIndex]);
            }
            else if (negativeOvertimeCounterToFilterOutTimeOffAndPayout + orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Hours <= 0M)
            {
                
                    negativeOvertimeCounterToFilterOutTimeOffAndPayout += orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Hours;
                
            }
            else if (negativeOvertimeCounterToFilterOutTimeOffAndPayout + orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Hours > 0M)
            {
                overtimeEntriesForPayoutCalculation.Add(new OvertimeEntryWithSalary(orderedOvertimeEntries[overtimeEntryIndex].SalaryPrHour, new OvertimeEntry
                {
                    CompensationRate = orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.CompensationRate,
                    Date = orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Date,
                    Hours = negativeOvertimeCounterToFilterOutTimeOffAndPayout + orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Hours,
                    Salary = orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.Salary,
                    TaskId = orderedOvertimeEntries[overtimeEntryIndex].OvertimeEntry.TaskId
                }));
                negativeOvertimeCounterToFilterOutTimeOffAndPayout = 0M;
            }
        }

        return (overtimeEntriesForPayoutCalculation, negativeOvertimeCounterToFilterOutTimeOffAndPayout);
    }

    private decimal  GetCalculatedPayoutForOvertimeEntries(List<List<OvertimeEntryWithSalary>> overtimeEntriesWithSalaryGroupedByCompRate, decimal requestedHoursForPayout)
    {
        var indexForComprateList = new List<(int indexOfComprate, decimal compRate)>();
        foreach (var overtimeEntriesWith in overtimeEntriesWithSalaryGroupedByCompRate)
        {
            var overtimeEntryWithSalary = overtimeEntriesWith.FirstOrDefault();
            if (overtimeEntryWithSalary!=null)
            {
                indexForComprateList.Add((overtimeEntriesWithSalaryGroupedByCompRate.IndexOf(overtimeEntriesWith), overtimeEntryWithSalary.OvertimeEntry.CompensationRate));
            }
        }

        indexForComprateList = indexForComprateList.OrderBy(x => x.compRate).ToList();
        var hoursForPayoutCounter = 0M;
        var overtimeEntriesForPayoutCalculation = new List<OvertimeEntryWithSalary>();
        for (int overtimeEntriesWithSalaryIndex = 0; overtimeEntriesWithSalaryIndex < indexForComprateList.Count; overtimeEntriesWithSalaryIndex++)
        {
            if (hoursForPayoutCounter == requestedHoursForPayout)
            {
                break;
            }

            var indexForCurrentCompRate = indexForComprateList[overtimeEntriesWithSalaryIndex].indexOfComprate;
            var overtimeEntriesWithSalaryOrderedByDate = overtimeEntriesWithSalaryGroupedByCompRate[indexForCurrentCompRate].OrderBy(x=>x.OvertimeEntry.Date).ToList();

            for (int overtimeEntryIndex = 0; overtimeEntryIndex < overtimeEntriesWithSalaryOrderedByDate.Count; overtimeEntryIndex++)
            {
                if (hoursForPayoutCounter == requestedHoursForPayout)
                {
                    break;
                }

                if (overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours +hoursForPayoutCounter >requestedHoursForPayout)
                {
                    var diff = requestedHoursForPayout - hoursForPayoutCounter;
                    var diffEntry = new OvertimeEntryWithSalary(
                        overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].SalaryPrHour, new OvertimeEntry
                        {
                            CompensationRate = overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry
                                .CompensationRate,
                            Date = overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry.Date,
                            Hours = diff,
                            Salary = overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry.Salary,
                            TaskId = overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry.TaskId
                        });
                    overtimeEntriesForPayoutCalculation.Add(diffEntry);
                    hoursForPayoutCounter += diff;
                }
                else if (overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours + hoursForPayoutCounter<=requestedHoursForPayout)
                {
                    hoursForPayoutCounter += overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex].OvertimeEntry.Hours;
                    overtimeEntriesForPayoutCalculation.Add(overtimeEntriesWithSalaryOrderedByDate[overtimeEntryIndex]);
                }
                
            }
        }
        
        return CalculatePayout(overtimeEntriesForPayoutCalculation);
    }
    
    private decimal CalculatePayout(List<OvertimeEntryWithSalary> overtimeEntriesWithSalary)
    {
        return overtimeEntriesWithSalary.Sum(overtimeEntry => overtimeEntry.OvertimeEntry.CompensationRate * overtimeEntry.OvertimeEntry.Hours * overtimeEntry.SalaryPrHour);
    }

    public decimal RegisterOvertimePayout(List<OvertimeEntry> overtimeEntries, int userId, GenericHourEntry requestedPayout, int paidOvertimeId)
    {
        var salaryData = _salaryService.GetEmployeeSalaryData(userId).OrderBy(x => x.FromDate).ToList();
        var overtimeEntriesByCompRateWithSalary = GetOvertimeEntriesWithSalaryGroupedByCompensationRate(salaryData, overtimeEntries);
        var positiveOvertimeEntriesForPayoutCalculation = GetOvertimeEntriesFilteredForPayoutsAndTimeOff(overtimeEntriesByCompRateWithSalary);//TODO: Eksisterer denne allerede?
        var salaryPayout = GetCalculatedPayoutForOvertimeEntries(positiveOvertimeEntriesForPayoutCalculation, requestedPayout.Hours);

        _salaryService.SaveOvertimePayout(
            new RegisterOvertimePayout(
                userId,
                requestedPayout.Date,
                salaryPayout,
                paidOvertimeId)
            );
        return salaryPayout;
    }
}