using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.Payouts;

public interface IPayoutStorage
{
    Task<PayoutsDto> GetRegisteredPayouts(PayoutQueryFilter criterias);

    Task<List<PayoutDto>> RegisterPayout(int userId, GenericPayoutHourEntry request,
        List<PayoutToRegister> payoutsToRegister);

    Task CancelPayout(DateTime payoutDate, User currentUser);
    Task<List<PayoutDto>> GetActivePayouts(int userId);
}

public class PayoutQueryFilter
{
    public DateTime? FromDateInclusive { get; set; }
    public DateTime? ToDateInclusive { get; set; }
    public int? UserId { get; set; }
}