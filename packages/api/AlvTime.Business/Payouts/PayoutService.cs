using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Overtime;

namespace AlvTime.Business.Payouts
{
    public class PayoutService
    {
        private readonly IPayoutStorage _payoutStorage;
        private readonly IUserContext _userContext;
        private readonly OvertimeService _overtimeService;

        public PayoutService(IPayoutStorage payoutStorage, IUserContext userContext, OvertimeService overtimeService)
        {
            _payoutStorage = payoutStorage;
            _userContext = userContext;
            _overtimeService = overtimeService;
        }

        public PayoutsDto GetRegisteredPayouts()
        {
            var currentUser = _userContext.GetCurrentUser();

            return _payoutStorage.GetRegisteredPayouts(currentUser.Id);
        }

        public PayoutDto RegisterPayout(GenericHourEntry request)
        {
            var currentUser = _userContext.GetCurrentUser();

            var availableHours = _overtimeService.GetAvailableOvertimeHours();
            var availableForPayout = availableHours.AvailableHoursBeforeCompensation;

            if (request.Hours <= availableForPayout)
            {
                var payoutHoursAfterCompensationRate = CalculatePayoutHoursBasedOnAvailableOvertime(request.Hours);
                return _payoutStorage.RegisterPayout(currentUser.Id, request, payoutHoursAfterCompensationRate);
            }

            throw new ValidationException("Not enough available hours");
        }

        public PayoutDto CancelPayout(int payoutId)
        {
            if (!PayoutCanBeDeleted(payoutId))
            {
                throw new ValidationException("Selected payout must be latest ordered payout");
            }

            return _payoutStorage.CancelPayout(payoutId);
        }

        private decimal CalculatePayoutHoursBasedOnAvailableOvertime(decimal requestedHours)
        {
            var availableHours = _overtimeService.GetAvailableOvertimeHours();
            
            var totalPayout = 0M;

            var orderedOverTime = availableHours.Entries.GroupBy(
                    hours => hours.CompensationRate,
                    hours => hours,
                    (cr, hours) => new
                    {
                        CompensationRate = cr,
                        Hours = hours.Sum(h => h.Hours)
                    })
                .OrderBy(h => h.CompensationRate);

            foreach (var entry in orderedOverTime)
            {
                if (requestedHours <= 0)
                {
                    break;
                }

                var hoursBeforeCompensation = Math.Min(requestedHours, entry.Hours);

                totalPayout += hoursBeforeCompensation * entry.CompensationRate;

                requestedHours -= hoursBeforeCompensation;
            }

            return totalPayout;
        }
        
        private bool PayoutCanBeDeleted(int payoutId)
        {
            var currentUser = _userContext.GetCurrentUser();
            var allActivePayouts = _payoutStorage.GetActivePayouts(currentUser.Id);

            if (!allActivePayouts.Any())
            {
                throw new ValidationException("There are no active payouts");
            }
            
            var payoutToBeCancelled = GetRegisteredPayouts().Entries.FirstOrDefault(po => po.Id == payoutId);

            if (payoutToBeCancelled == null)
            {
                throw new ValidationException("Payout id does not exist");
            }
            if (!payoutToBeCancelled.Active)
            {
                throw new ValidationException("Payout is not active");
            }
            
            var latestId = allActivePayouts.OrderBy(p => p.Id).Last().Id;

            if (payoutId < latestId)
            {
                return false;
            }

            return true;
        }
    }
}