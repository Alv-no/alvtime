using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.Payouts
{
    public interface IPayoutStorage
    {
        PayoutsDto GetRegisteredPayouts(PayoutQueryFilter criterias);
        PayoutDto RegisterPayout(int userId, GenericHourEntry request, decimal payoutHoursAfterCompRate);
        PayoutDto CancelPayout(int payoutId);
        List<PayoutDto> GetActivePayouts(int userId);
    }

    public class PayoutQueryFilter
    {
        public DateTime? FromDateInclusive { get; set; }
        public DateTime? ToDateInclusive { get; set; }
        public int? UserId { get; set; }
    }
}