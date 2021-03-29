using System;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        AvailableHoursDto GetAvailableHours(int userId, DateTime startDate, DateTime endDate);
        FlexedHoursDto GetFlexedHours(int userId);
        PayoutsDto GetRegisteredPayouts(int userId);
        PaidOvertimeEntry RegisterPaidOvertime(GenericHourEntry request, int userId);
        PaidOvertimeEntry CancelPayout(int userId, int id);

    }
}
