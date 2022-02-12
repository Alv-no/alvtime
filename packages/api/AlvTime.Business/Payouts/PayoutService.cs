using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using FluentValidation;

namespace AlvTime.Business.Payouts
{
    public class PayoutService
    {
        private readonly IPayoutStorage _payoutStorage;
        private readonly IUserContext _userContext;
        private readonly TimeRegistrationService _timeRegistrationService;

        public PayoutService(IPayoutStorage payoutStorage, IUserContext userContext, TimeRegistrationService timeRegistrationService)
        {
            _payoutStorage = payoutStorage;
            _userContext = userContext;
            _timeRegistrationService = timeRegistrationService;
        }

        public PayoutsDto GetRegisteredPayouts()
        {
            var currentUser = _userContext.GetCurrentUser();

            return _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter{ UserId = currentUser.Id });
        }

        public PayoutDto RegisterPayout(GenericHourEntry request)
        {
            var currentUser = _userContext.GetCurrentUser();

            var availableHours = _timeRegistrationService.GetAvailableOvertimeHoursAtDate(request.Date.Date);
            var availableForPayout = availableHours.AvailableHoursBeforeCompensation;

            if (request.Hours <= availableForPayout)
            {
                var listOfPayoutsToRegister = CalculatePayoutHoursBasedOnAvailableOvertime(request.Hours, availableHours);
                var registeredPayouts = _payoutStorage.RegisterPayout(currentUser.Id, request, listOfPayoutsToRegister);

                return new PayoutDto
                {
                    Id = 0,
                    UserId = registeredPayouts.First().UserId,
                    Date = request.Date,
                    HoursBeforeCompensation = registeredPayouts.Sum(payout => payout.HoursBeforeCompensation),
                    HoursAfterCompensation = registeredPayouts.Sum(payout => payout.HoursAfterCompensation)
                };
            }

            throw new ValidationException("Ikke nok tilgjengelige timer.");
        }

        public PayoutDto CancelPayout(int payoutId)
        {
            if (!PayoutCanBeDeleted(payoutId))
            {
                throw new ValidationException("Valgt utbetaling må være seneste bestilte utbetaling.");
            }

            return _payoutStorage.CancelPayout(payoutId);
        }

        private List<PayoutToRegister> CalculatePayoutHoursBasedOnAvailableOvertime(decimal requestedHours, AvailableOvertimeDto availableHours)
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
        
        private bool PayoutCanBeDeleted(int payoutId)
        {
            var currentUser = _userContext.GetCurrentUser();
            var allActivePayouts = _payoutStorage.GetActivePayouts(currentUser.Id);

            if (!allActivePayouts.Any())
            {
                throw new ValidationException("Det er ingen aktive utbetalinger.");
            }
            
            var payoutToBeCancelled = GetRegisteredPayouts().Entries.FirstOrDefault(po => po.Id == payoutId);

            if (payoutToBeCancelled == null)
            {
                throw new ValidationException("Utbetaling finnes ikke.");
            }
            if (!payoutToBeCancelled.Active)
            {
                throw new ValidationException("Utbetaling er ikke aktiv.");
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