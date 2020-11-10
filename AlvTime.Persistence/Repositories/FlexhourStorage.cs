using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
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

    public IEnumerable<FlexiHours> GetFlexihours(DateTime startDate, DateTime endDate, int userId)
    {
        var flexHours = new List<FlexiHours>();
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);
        var entriesByDate = GetTimeEntries(startDate, endDate, userId);

        foreach (var currentDate in GetWorkingDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day.GetWorkingHours() != HoursInRegularWorkday && dbUser.StartDate <= day.Date)
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
        var registeredPayouts = GetRegisteredPayouts(startDate, endDate, userId);

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, endDate, userId);
        CompensateForRegisteredPayouts(overtimeEntries, registeredPayouts);

        return overtimeEntries.Sum(h => h.CompensationRate * h.Value);
    }

    private List<DateEntry> GetTimeEntries(DateTime startDate, DateTime endDate, int userId)
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
                        Value = Math.Min(overtimeHours, entry.Value),
                        CompensationRate = entry.CompensationRate,
                        Date = day.Date
                    };

                    overtimeEntries.Add(overtimeEntry);
                    overtimeHours -= overtimeEntry.Value;
                }
            }
        }

        return overtimeEntries;
    }

    private void CompensateForOffTime(List<OvertimeEntry> overtimeEntries, IEnumerable<DateEntry> entriesByDate, DateTime startDate, DateTime endDate, int userId)
    {
        var dbUser = _context.User.SingleOrDefault(u => u.Id == userId);

        foreach (var currentDate in GetWorkingDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day != null && day.GetWorkingHours() < HoursInRegularWorkday && dbUser.StartDate <= day.Date)
            {
                var hoursOff = HoursInRegularWorkday - day.GetWorkingHours();

                var orderedOverTime = overtimeEntries.GroupBy(
                    hours => hours.CompensationRate,
                    hours => hours,
                    (cr, hours) => new
                    {
                        CompensationRate = cr,
                        Hours = hours.Sum(h => h.Value)
                    })
                    .OrderByDescending(h => h.CompensationRate);

                foreach (var entry in orderedOverTime)
                {
                    if (hoursOff <= 0)
                    {
                        break;
                    }

                    OvertimeEntry overtimeEntry = new OvertimeEntry
                    {
                        Value = -Math.Min(hoursOff, entry.Hours),
                        CompensationRate = entry.CompensationRate,
                        Date = day.Date
                    };

                    overtimeEntries.Add(overtimeEntry);
                    hoursOff -= overtimeEntry.Value;
                }
            }
        }
    }

    private void CompensateForRegisteredPayouts(List<OvertimeEntry> overtimeEntries, IEnumerable<RegisterPaidOvertimeDto> registeredPayouts)
    {
        var registeredPayoutsTotal = registeredPayouts.Sum(payout => payout.Value);

        var orderedOverTime = overtimeEntries.GroupBy(
            hours => hours.CompensationRate,
            hours => hours,
            (cr, hours) => new
            {
                CompensationRate = cr,
                Hours = hours.Sum(h => h.Value)
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
                Value = -Math.Min(registeredPayoutsTotal, entry.Hours),
                CompensationRate = entry.CompensationRate,
                Date = new DateTime(2020, 01, 01)
            };

            overtimeEntries.Add(overtimeEntry);
            registeredPayoutsTotal += overtimeEntry.Value;
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
        var currentYear = request.Date.Year;

        var availableOvertimeEquivalents = GetOvertimeEquivalents(new DateTime(currentYear, 01, 01), request.Date, userId);

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

    public IEnumerable<RegisterPaidOvertimeDto> GetRegisteredPayouts(DateTime fromDateInclusive, DateTime toDateInclusive, int userId)
    {
        var hours = _context.PaidOvertime.AsQueryable()
                    .Filter(new OvertimePayoutQuerySearch
                    {
                        FromDateInclusive = fromDateInclusive,
                        ToDateInclusive = toDateInclusive,
                        UserId = userId
                    })
                    .Select(x => new RegisterPaidOvertimeDto
                    {
                        Value = x.Value,
                        Date = x.Date,
                    })
                    .ToList();

        return hours;
    }

    private class OvertimeEntry
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal CompensationRate { get; set; }
    }
}
