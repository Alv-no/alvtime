using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using System;
using System.Collections.Generic;
using System.Linq;

public class FlexhourCalculator : IFlexhourCalculator
{
    private const decimal HoursInRegularWorkday = 7.5M;
    private readonly ITimeEntryStorage _storage;

    public FlexhourCalculator(ITimeEntryStorage storage)
    {
        _storage = storage;
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
}
