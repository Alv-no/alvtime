using System;
using System.Collections.Generic;

namespace AlvTime.Business.Overtime
{
    public interface IOvertimeStorage
    {
        List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias);
        void StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId);
        void DeleteOvertimeOnDate(DateTime date, int userId);
    }

    public class OvertimeQueryFilter
    {
        public int? UserId { get; set; }
        public DateTime? Date { get; set; }
        public decimal? CompensationRate { get; set; }
    }
}