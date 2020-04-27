using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.FlexiHours.FlexiHourStorage
{
    public class FlexiHourStorage : IFlexiHourStorage
    {
        private readonly AlvTime_dbContext _context;

        public FlexiHourStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public FlexiHourResponseDto GetTotalFlexiHours()
        {
            var calculator = new AlvHoursCalculator();

            return new FlexiHourResponseDto
            {
                FlexiHours = 187.5M + calculator.CalculateAlvHours()
            };
        }

        public FlexiHourResponseDto GetUsedFlexiHours(int userId)
        {
            var currentYear = DateTime.UtcNow.Year;

            decimal totalUsedHours = 0;

            var hourList = _context.Hours
                .Where(x => x.User == userId && x.TaskId == 13 && x.Year == currentYear)
                .ToList();

            foreach (var timeEntry in hourList)
            {
                totalUsedHours += timeEntry.Value;
            }

            return new FlexiHourResponseDto
            {
                FlexiHours = totalUsedHours
            };
        }
    }
}
