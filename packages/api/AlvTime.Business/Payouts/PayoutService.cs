using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;

namespace AlvTime.Business.Payouts;

public class PayoutService
{
    private readonly IPayoutStorage _payoutStorage;
    private readonly IUserContext _userContext;
    private readonly TimeRegistrationService _timeRegistrationService;
    private readonly PayoutValidationService _payoutValidationService;

    public PayoutService(IPayoutStorage payoutStorage, IUserContext userContext,
        TimeRegistrationService timeRegistrationService, PayoutValidationService payoutValidationService)
    {
        _payoutStorage = payoutStorage;
        _userContext = userContext;
        _timeRegistrationService = timeRegistrationService;
        _payoutValidationService = payoutValidationService;
    }

    public async Task<PayoutsDto> GetRegisteredPayouts()
    {
        var currentUser = await _userContext.GetCurrentUser();

        return await _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter { UserId = currentUser.Id });
    }

    public async Task<PayoutDto> RegisterPayout(GenericPayoutHourEntry request)
    {
        var currentUser = await _userContext.GetCurrentUser();

        await _payoutValidationService.ValidatePayout(request, currentUser.Id);

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(request.Date.Date);

        var listOfPayoutsToRegister =
            CalculatePayoutHoursBasedOnAvailableOvertime(request.Hours, availableHours);
        var registeredPayouts =
            await _payoutStorage.RegisterPayout(currentUser.Id, request, listOfPayoutsToRegister);

        return new PayoutDto
        {
            Id = 0,
            UserId = registeredPayouts.First().UserId,
            Date = request.Date,
            HoursBeforeCompensation = registeredPayouts.Sum(payout => payout.HoursBeforeCompensation),
            HoursAfterCompensation = registeredPayouts.Sum(payout => payout.HoursAfterCompensation)
        };
    }

    public async Task CancelPayout(DateTime payoutDate)
    {
        var date = payoutDate.Date;
        var currentUser = await _userContext.GetCurrentUser();
        await _payoutValidationService.ValidatePayoutCancellation(date, currentUser.Id);
        await _payoutStorage.CancelPayout(date, currentUser);
    }

    private List<PayoutToRegister> CalculatePayoutHoursBasedOnAvailableOvertime(decimal requestedHours,
        AvailableOvertimeDto availableHours)
    {
        var listOfPayouts = new List<PayoutToRegister>();

        var orderedOvertimeByRate = availableHours.Entries.GroupBy(
                hours => hours.CompensationRate,
                hours => hours,
                (cr, hours) => new
                {
                    CompensationRate = cr,
                    Hours = hours.Sum(h => h.Hours)
                })
            .OrderBy(h => h.CompensationRate);

        foreach (var entry in orderedOvertimeByRate)
        {
            if (requestedHours <= 0)
            {
                break;
            }

            var hoursBeforeCompensation = Math.Min(requestedHours, entry.Hours);

            requestedHours -= hoursBeforeCompensation;
            var payoutToRegister = new PayoutToRegister
            {
                HoursBeforeCompRate = hoursBeforeCompensation,
                HoursAfterCompRate = hoursBeforeCompensation * entry.CompensationRate,
                CompRate = entry.CompensationRate
            };
            listOfPayouts.Add(payoutToRegister);
        }

        return listOfPayouts;
    }

    public async Task<int> LockPayments(DateTime lockDate)
    {
        return await _payoutStorage.SetPaymentsToLocked(new PayoutQueryFilter(), lockDate.Date);
    }
}