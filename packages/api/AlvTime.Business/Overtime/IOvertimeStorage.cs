using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.Overtime
{
    public interface IOvertimeStorage
    {
        List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias);
        void StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId);
        void DeleteOvertimeOnDate(DateTime date, int userId);
        AvailableHoursDto GetAvailableHours(int userId);
    }

    public class OvertimeQueryFilter
    {
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? CompensationRate { get; set; }
    }
}