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

    public IEnumerable<FlexiHours> GetFlexihours(DateTime startDate, DateTime endDate, int userId)
    {
        var flexHours = new List<FlexiHours>();

        var entriesByDate = _storage.GetDateEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = startDate,
            ToDateInclusive = endDate
        });

        foreach (var currentDate in GetWorkingDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day == null)
            {
                flexHours.Add(new FlexiHours
                {
                    Value = -HoursInRegularWorkday,
                    Date = currentDate
                });
            }
            else if (day.GetWorkingHours() != HoursInRegularWorkday)
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
        var entriesByDate = _storage.GetDateEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = startDate,
            ToDateInclusive = endDate
        });

        List<OvertimeEntry> overtimeEntries = GetOvertimeEntries(entriesByDate, startDate, endDate);
        CompensateForOffTime(overtimeEntries, entriesByDate, startDate, endDate);

        return overtimeEntries.Sum(h => h.CompensationRate * h.Value);
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

    private void CompensateForOffTime(List<OvertimeEntry> overtimeEntries, IEnumerable<DateEntry> entriesByDate, DateTime startDate, DateTime endDate)
    {
        foreach (var currentDate in GetWorkingDaysInPeriod(startDate, endDate))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (day != null && day.GetWorkingHours() < HoursInRegularWorkday)
            {
                var hoursOff = (HoursInRegularWorkday - day.GetWorkingHours());

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

    public RegisterPaidOvertimeDto RegisterPaidOvertime(DateTime date, decimal valueRegistered, int userId)
    {
        PaidOvertime paidOvertime = new PaidOvertime
        {
            Date = date,
            User = userId,
            Value = valueRegistered
        };

        _context.PaidOvertime.Add(paidOvertime);
        _context.SaveChanges();

        return new RegisterPaidOvertimeDto
        {
            Date = paidOvertime.Date,
            Value = paidOvertime.Value
        };
    }

    private class OvertimeEntry
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal CompensationRate { get; set; }
    }
}
