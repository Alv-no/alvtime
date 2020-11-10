using System;
using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        IEnumerable<FlexiHours> GetFlexihours(DateTime fromDate, DateTime toDate, int userId);
        decimal GetOvertimeEquivalents(DateTime fromDate, DateTime toDate, int userId);
        RegisterPaidOvertimeDto RegisterPaidOvertime(RegisterPaidOvertimeDto request, int userId);
        IEnumerable<RegisterPaidOvertimeDto> GetRegisteredPayouts(DateTime fromDateInclusive, DateTime toDateInclusive, int userId);
    }

    public class OvertimePayoutQuerySearch
    {
        public int? UserId { get; set; }
        public DateTime? FromDateInclusive { get; set; }
        public DateTime? ToDateInclusive { get; set; }
    }
}
