using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.Payouts
{
    public interface IPayoutStorage
    {
        PayoutsDto GetRegisteredPayouts(PayoutQueryFilter criterias);
        List<PayoutDto> RegisterPayout(int userId, GenericHourEntry request, List<PayoutToRegister> payoutsToRegister);
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