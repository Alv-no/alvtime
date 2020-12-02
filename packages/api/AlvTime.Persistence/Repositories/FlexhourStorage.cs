using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

public class FlexhourStorage : IFlexhourStorage
{
    private const decimal HoursInRegularWorkday = 7.5M;
    private readonly ITimeEntryStorage _storage;
    private readonly AlvTime_dbContext _context;
    private readonly int _flexTask; //To be implemented as env variable

    public FlexhourStorage(ITimeEntryStorage storage, AlvTime_dbContext context)
    {
        _storage = storage;
        _context = context;
    }

    public AvailableHoursDto GetAvailableHours(int userId)
    {
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);
        var startDate = dbUser.StartDate;
        var endDate = DateTime.Now.Date;

        var entriesByDate = GetTimeEntries(startDate, endDate, userId);
        var overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);
        (var overtime, var flex) = GetFlexAndOvertime(startDate, endDate, userId);

        return new AvailableHoursDto
        {
            TotalHours = flex,
            TotalHoursIncludingCompensationRate = overtime,
            Entries = overtimeEntries
        };
    }

    public FlexedHoursDto GetFlexedHours(int userId)
    {
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);
        var startDate = dbUser.StartDate;
        var endDate = DateTime.Now.Date;

        var timeOffEntries = _context.Hours.Where(h => h.User == userId && h.TaskId == 18);
        var timeOffSum = _context.Hours.Where(h => h.User == userId && h.TaskId == 18).Sum(h => h.Value);

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

    public (decimal overtime, decimal flex) GetFlexAndOvertime(DateTime startDate, DateTime endDate, int userId)
    {
        List<DateEntry> entriesByDate = GetTimeEntries(startDate, endDate, userId);
        var registeredPayouts = GetRegisteredPayouts(userId);

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, endDate, userId);
        CompensateForRegisteredPayouts(overtimeEntries, registeredPayouts);

        var sumCompensatedHours = overtimeEntries.Sum(o => o.CompensationRate * o.Hours);
        var sumFlexHours = overtimeEntries.Sum(o => o.Hours);

        return (sumCompensatedHours > 0 ? sumCompensatedHours : 0, sumFlexHours);
    }

    public List<DateEntry> GetTimeEntries(DateTime startDate, DateTime endDate, int userId)
    {
        var entriesByDate = _storage.GetDateEntries(new TimeEntryQuerySearch
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
        if ((day.Date.DayOfWeek == DayOfWeek.Sunday ||
            day.Date.DayOfWeek == DayOfWeek.Saturday ||
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
            var entriesWithTimeOff = day.Entries.Where(e => e.TaskId == 18);

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
                if (!orderedOverTime.Any() || hoursOffThisDay > 0)
                {
                    OvertimeEntry overtimeEntry = new OvertimeEntry
                    {
                        Hours = -hoursOffThisDay,
                        CompensationRate = 1,
                        Date = day.Date
                    };
                    overtimeEntries.Add(overtimeEntry);
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
        var startDate = user.StartDate;

        List<DateEntry> entriesByDate = GetTimeEntries(startDate, request.Date, userId);
        var registeredPayouts = GetRegisteredPayouts(userId);

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, request.Date);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, request.Date, userId);
        CompensateForRegisteredPayouts(overtimeEntries, registeredPayouts);

        var availableForPayout = overtimeEntries.Sum(ot => ot.Hours);

        var hoursAfterCompRate = HoursAfterCompRate(overtimeEntries, request.Hours);

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

    private decimal HoursAfterCompRate(List<OvertimeEntry> overtimeEntries, decimal orderedHours)
    {
        var calculatedHours = 0M;

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

            var smallerValue = Math.Min(orderedHours * entry.CompensationRate, entry.Hours * entry.CompensationRate);

            calculatedHours += smallerValue;

            orderedHours -= smallerValue / entry.CompensationRate;
        }

        return calculatedHours;
    }
}
