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
            var existingPayoutsOnDate = GetRegisteredPayouts().Entries.Where(e => e.Date.Date == request.Date.Date);

            if (existingPayoutsOnDate.Any())
            {
                throw new ValidationException("Du har allerede bestilt en utbetaling denne dagen. Kanseller den forrige og bestill på nytt.");
            }
            
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

        public void CancelPayout(DateTime payoutDate)
        {
            var date = payoutDate.Date;
            ValidatePayoutCancellation(date);
            _payoutStorage.CancelPayout(date);
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
        
        private void ValidatePayoutCancellation(DateTime payoutDate)
        {
            var currentUser = _userContext.GetCurrentUser();
            var allActivePayouts = _payoutStorage.GetActivePayouts(currentUser.Id);

            if (!allActivePayouts.Any())
            {
                throw new ValidationException("Det er ingen aktive utbetalinger.");
            }
            
            var payoutsToBeCancelled = GetRegisteredPayouts().Entries.Where(po => po.Date.Date == payoutDate).ToList();

            if (!payoutsToBeCancelled.Any())
            {
                throw new ValidationException("Utbetaling finnes ikke.");
            }
            if (!payoutsToBeCancelled.First().Active)
            {
                throw new ValidationException("Utbetaling er ikke aktiv.");
            }
            
            var mostRecentPayoutOrderDate = allActivePayouts.OrderBy(p => p.Date).Last().Date;

            if (payoutsToBeCancelled.First().Date < mostRecentPayoutOrderDate)
            {
                throw new ValidationException("Valgt utbetaling må være seneste bestilte utbetaling.");
            }
        }
    }
}