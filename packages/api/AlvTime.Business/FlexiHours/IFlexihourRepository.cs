using Microsoft.AspNetCore.Mvc;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        AvailableHoursDto GetAvailableHours(int userId);
        FlexedHoursDto GetFlexedHours(int userId);
        PayoutsDto GetRegisteredPayouts(int userId);
        ObjectResult RegisterPaidOvertime(GenericHourEntry request, int userId);
        PaidOvertimeEntry CancelPayout(int userId, int id);

    }
}
