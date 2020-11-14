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
}
