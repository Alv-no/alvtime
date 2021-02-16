using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public FlexhourStorage(ITimeEntryStorage timeEntryStorage, AlvTime_dbContext context, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
    {
        _timeEntryStorage = timeEntryStorage;
        _context = context;
        _timeEntryOptions = timeEntryOptions;
        _flexTask = _timeEntryOptions.CurrentValue.FlexTask;
        _paidHolidayTask = _timeEntryOptions.CurrentValue.PaidHolidayTask;
        _unpaidHolidayTask = _timeEntryOptions.CurrentValue.UnpaidHolidayTask;
        _startOfOvertimeSystem = _timeEntryOptions.CurrentValue.StartOfOvertimeSystem;
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
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);
        var startDate = dbUser.StartDate;
        var endDate = DateTime.Now.Date;

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

    public List<OvertimeEntry> GetOvertimeEntriesAfterOffTimeAndPayouts(DateTime startDate, DateTime endDate, int userId)
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
        var overtimeEntries = new List<OvertimeEntry>();

        var overtimeHours = isRedDay ? day.GetWorkingHours() : day.GetWorkingHours() - HoursInRegularWorkday;

        foreach (var entry in day.Entries.OrderBy(task => task.CompensationRate))
        {
            if (overtimeHours <= 0)
            {
                break;
            }

            if ((isRedDay || IsWeekend(day)) && (entry.TaskId == _paidHolidayTask || entry.TaskId == _unpaidHolidayTask))
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

    private bool IsWeekend(DateEntry entry)
    {
        return entry.Date.DayOfWeek == DayOfWeek.Saturday || entry.Date.DayOfWeek == DayOfWeek.Saturday;
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

    private void CompensateForOffTime(List<OvertimeEntry> overtimeEntries, IEnumerable<DateEntry> entriesByDate, DateTime startDate, DateTime endDate, int userId)
    {
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);

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
        var registeredPayoutsTotal = registeredPayouts.TotalHours;

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
            if (registeredPayoutsTotal <= 0)
            {
                break;
            }

            OvertimeEntry overtimeEntry = new OvertimeEntry
            {
                Hours = -Math.Min(registeredPayoutsTotal, entry.Hours),
                CompensationRate = entry.CompensationRate,
                Date = DateTime.Now
            };

            overtimeEntries.Add(overtimeEntry);
            registeredPayoutsTotal += overtimeEntry.Hours;
        }
    }

    private IEnumerable<DateTime> GetDaysInPeriod(DateTime from, DateTime to)
    {
        for (DateTime currentDate = from; currentDate <= to; currentDate += TimeSpan.FromDays(1))
        {
            yield return currentDate;
        }
    }

    public ObjectResult RegisterPaidOvertime(GenericHourEntry request, int userId)
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

            return new OkObjectResult(paidOvertime);
        }

        return new BadRequestObjectResult("Not enough available hours");
    }

    public PaidOvertimeEntry CancelPayout(int userId, int id)
    {
        var payout = _context.PaidOvertime.FirstOrDefault(po => po.Id == id && po.User == userId);

        if (payout != null && payout.Date.Month >= DateTime.Now.Month && payout.Date.Year == DateTime.Now.Year)
        {
            _context.PaidOvertime.Remove(payout);
            _context.SaveChanges();

            return new PaidOvertimeEntry
            {
                Date = payout.Date,
                Id = payout.Id,
                UserId = payout.User,
                Value = payout.HoursBeforeCompRate
            };
        }

        return new PaidOvertimeEntry();
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
}
