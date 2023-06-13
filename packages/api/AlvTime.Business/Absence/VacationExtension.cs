using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.TimeRegistration;

namespace AlvTime.Business.Absence
{
    public static class VacationExtension
    {
        public static VacationOverviewDto CalculateVacationOverview(IEnumerable<TimeEntryResponseDto> entries)
        {
            var sumHours = entries.Sum(e => e.Value);

            var totalHoursUsed = 0M;
            var totalDaysUsed = 0;

            if (sumHours % 7.5M == 0)
            {
                totalDaysUsed = (int)(sumHours / 7.5M);
            }
            else
            {
                totalDaysUsed = (int)Math.Floor(sumHours / 7.5M);
                totalHoursUsed = sumHours % 7.5M;
            }

            return new VacationOverviewDto
            {
                Entries = entries,
                TotalDaysUsed = totalDaysUsed,
                TotalHoursUsed = totalHoursUsed
            };
        }
    }
}
