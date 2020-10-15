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

        for (DateTime currentDate = startDate; currentDate <= endDate; currentDate += TimeSpan.FromDays(1))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (!(currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday))
            {
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
        }

        return flexHours;
    }

    public decimal GetOvertimeEquivalents(DateTime startDate, DateTime endDate, int userId)
    {
        var flexHours = new List<FlexiHours>();
        var sum = 0M;

        var entriesByDate = _storage.GetDateEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = startDate,
            ToDateInclusive = endDate
        });

        var list = new List<OvertimeEntry>();

        // [{date, compensationRate, hours}, {}]

        for (DateTime currentDate = startDate; currentDate <= endDate; currentDate += TimeSpan.FromDays(1))
        {
            var day = entriesByDate.SingleOrDefault(entryDate => entryDate.Date == currentDate);

            if (!(currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday))
            {
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

                    if(day.GetWorkingHours() < HoursInRegularWorkday)
                    {
                        sum += day.GetWorkingHours() - HoursInRegularWorkday;
                    }
                    else
                    {
                        sum += GetCompensation(day);
                    }
                }
            }
        }

        return sum;
    }

    private static decimal GetCompensation(DateEntry day)
    {
        decimal compensation = 0;
        var overtimeHours = (day.GetWorkingHours() - HoursInRegularWorkday);

        var orderedEntries = day.Entries.OrderBy(task => task.CompensationRate);
        foreach (var entry in orderedEntries)
        {
            if (overtimeHours <= 0)
            {
                break;
            }

            var compensationHours = Math.Min(overtimeHours, entry.Value);
            compensation += compensationHours * entry.CompensationRate;

            overtimeHours -= compensationHours;
        }

        return compensation;
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
