using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.Payouts
{
    public interface IPayoutStorage
    {
        PayoutsDto GetRegisteredPayouts(int userId);
        PayoutDto RegisterPayout(int userId, GenericHourEntry request, decimal payoutHoursAfterCompRate);
        PayoutDto CancelPayout(int payoutId);
        List<PayoutDto> GetActivePayouts(int userId);
    }
}