using Microsoft.AspNetCore.Mvc;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        AvailableHoursDto GetAvailableHours(int userId);
        FlexedHoursDto GetFlexedHours(int userId);
        PayoutsDto GetRegisteredPayouts(int userId);
        ObjectResult RegisterPaidOvertime(RegisterPaidOvertimeDto request, int userId);
    }
}
