using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public class FlexiHourStorage : IFlexiHourStorage
    {
        private readonly AlvTime_dbContext _context;
        private readonly ITimeEntryStorage _timeEntryStorage;

        public FlexiHourStorage(AlvTime_dbContext context, ITimeEntryStorage timeEntryStorage)
        {
            _context = context;
            _timeEntryStorage = timeEntryStorage;
        }

        public IEnumerable<FlexiHoursResponseDto> GetFlexiHours(int userId, DateTime startDate, DateTime endDate)
        {
            var flexHours = new List<FlexiHoursResponseDto>();

            var timeEntries = _timeEntryStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = userId,
                FromDateInclusive = startDate,
                ToDateInclusive = endDate
            });

            var hoursByDate = timeEntries.GroupBy(
                    h => h.Date,
                    h => h.Value,
                    (date, value) => new
                    {
                        Date = date,
                        SumHours = value.ToList().Sum()
                    });

            foreach (var hour in hoursByDate)
            {

                if (hour.SumHours != 7.5M)
                {
                    flexHours.Add(new FlexiHoursResponseDto
                    {
                        Value = hour.SumHours - 7.5M,
                        Date = DateTime.Parse(hour.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    });
                }
            }

            return flexHours;
        }
    }
}
