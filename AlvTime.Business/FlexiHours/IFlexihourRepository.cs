using System;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        AvailableHoursDto GetAvailableHours(int userId);
        FlexedHoursDto GetFlexedHours(int userId);
        PayoutsDto GetRegisteredPayouts(int userId);
        RegisterPaidOvertimeDto RegisterPaidOvertime(RegisterPaidOvertimeDto request, int userId);
    }

    public class OvertimePayoutQuerySearch
    {
        public int? UserId { get; set; }
        public DateTime? FromDateInclusive { get; set; }
        public DateTime? ToDateInclusive { get; set; }
    }
}
