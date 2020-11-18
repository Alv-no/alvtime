using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;

public class FlexhourStorage : IFlexhourStorage
{
    private const decimal HoursInRegularWorkday = 7.5M;
    private readonly ITimeEntryStorage _storage;
    private readonly AlvTime_dbContext _context;

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

        var timeOff = _context.Hours.Where(h => h.User == userId && h.TaskId == 18).Sum(h => h.Value);
        var hoursWorkedOver = GetHoursWorkedMoreThanWorkday(startDate, endDate, userId).Sum(e => e.Value) - timeOff;

        var hoursWithCompRate = GetOvertimeEquivalents(startDate, endDate, userId);

        var entriesByDate = GetTimeEntries(startDate, endDate, userId);

        var overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);

        return new AvailableHoursDto
        {
            TotalHours = hoursWorkedOver,
            TotalHoursIncludingCompensationRate = hoursWithCompRate,
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
        var sumPayouts = payouts.Sum(po => po.Value);

        return new PayoutsDto 
        {
            TotalHours = sumPayouts,
            Entries = payouts.Select(po => new GenericHourEntry
            {
                Date = po.Date,
                Hours = po.Value
            }).ToList()
        };
    }

    public List<FlexiHours> GetHoursWorkedMoreThanWorkday(DateTime startDate,DateTime endDate, int userId)
    {
        var flexHours = new List<FlexiHours>();
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);
        var entriesByDate = GetTimeEntries(startDate, endDate, userId);

        foreach (var currentDate in GetWorkingDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day.GetWorkingHours() > HoursInRegularWorkday)
            {
                flexHours.Add(new FlexiHours
                {
                    Value = day.GetWorkingHours() - HoursInRegularWorkday,
                    Date = day.Date
                });
            }
        }

        return flexHours;
    }

    public decimal GetOvertimeEquivalents(DateTime startDate, DateTime endDate, int userId)
    {
        List<DateEntry> entriesByDate = GetTimeEntries(startDate, endDate, userId);
        var registeredPayouts = GetRegisteredPayouts(userId);

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, endDate, userId);
        CompensateForRegisteredPayouts(overtimeEntries, registeredPayouts);

        return overtimeEntries.Sum(h => h.CompensationRate * h.Hours);
    }

    public List<DateEntry> GetTimeEntries(DateTime startDate, DateTime endDate, int userId)
    {
        var entriesByDate = _storage.GetDateEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = startDate,
            ToDateInclusive = endDate
        }).ToList();

        var daysNotRecorded = GetWorkingDaysInPeriod(startDate, endDate).Where(day => !entriesByDate.Select(e => e.Date).Contains(day));
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

        foreach (var currentDate in GetWorkingDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day != null && day.GetWorkingHours() > HoursInRegularWorkday)
            {
                var overtimeHours = day.GetWorkingHours() - HoursInRegularWorkday;

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

                    overtimeEntries.Add(overtimeEntry);
                    overtimeHours -= overtimeEntry.Hours;
                }
            }
        }

        return overtimeEntries;
    }

    private void CompensateForOffTime(List<OvertimeEntry> overtimeEntries, IEnumerable<DateEntry> entriesByDate, DateTime startDate, DateTime endDate, int userId)
    {
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);

        foreach (var currentWorkDay in GetWorkingDaysInPeriod(startDate, endDate))
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
                    hoursOffThisDay -= overtimeEntry.Hours;
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
            .OrderByDescending(h => h.CompensationRate);

        foreach (var entry in orderedOverTime)
        {
            if (registeredPayoutsTotal <= 0)
            {
                break;
            }

            OvertimeEntry overtimeEntry = new OvertimeEntry
            {
                Hours = -Math.Min(registeredPayoutsTotal, entry.Hours * entry.CompensationRate),
                CompensationRate = 1,
                Date = DateTime.Now
            };

            overtimeEntries.Add(overtimeEntry);
            registeredPayoutsTotal += overtimeEntry.Hours;
        }
    }

    private IEnumerable<DateTime> GetWorkingDaysInPeriod(DateTime from, DateTime to)
    {
        for (DateTime currentDate = from; currentDate <= to; currentDate += TimeSpan.FromDays(1))
        {
            if (!(currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday))
            {
                yield return currentDate;
            }
        }
    }

    public RegisterPaidOvertimeDto RegisterPaidOvertime(RegisterPaidOvertimeDto request, int userId)
    {
        var user = _context.User.SingleOrDefault(u => u.Id == userId);
        var startDate = user.StartDate;

        List<DateEntry> entriesByDate = GetTimeEntries(startDate, request.Date, userId);
        var registeredPayouts = GetRegisteredPayouts(userId);

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, request.Date);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, request.Date, userId);
        CompensateForRegisteredPayouts(overtimeEntries, registeredPayouts);

        var availableOvertimeEquivalents = GetOvertimeEquivalents(startDate, request.Date, userId);

        if (request.Value <= availableOvertimeEquivalents)
        {
            PaidOvertime paidOvertime = new PaidOvertime
            {
                Date = request.Date,
                User = userId,
                Value = request.Value
            };

            _context.PaidOvertime.Add(paidOvertime);
            _context.SaveChanges();

            return request;
        }

        return new RegisterPaidOvertimeDto();
    }
}
