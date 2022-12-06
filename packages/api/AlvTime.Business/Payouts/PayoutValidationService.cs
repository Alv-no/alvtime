using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using FluentValidation;

namespace AlvTime.Business.Payouts;

public class PayoutValidationService
{
    private readonly UserService _userService;
    private const decimal HoursInWorkDay = 7.5M;
    private readonly TimeRegistrationService _timeRegistrationService;
    private readonly IPayoutStorage _payoutStorage;

    public PayoutValidationService(UserService userService, TimeRegistrationService timeRegistrationService, IPayoutStorage payoutStorage)
    {
        _userService = userService;
        _timeRegistrationService = timeRegistrationService;
        _payoutStorage = payoutStorage;
    }

    public async Task ValidatePayout(GenericHourEntry request, int userId)
    {
        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(request.Date.Date);
        var availableForPayout = availableHours.AvailableHoursBeforeCompensation;
        var existingPayoutsOnDate =
            (await _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter { UserId = userId })).Entries.Where(e => e.Date.Date == request.Date.Date);

        if (existingPayoutsOnDate.Any())
        {
            throw new ValidationException(
                "Du har allerede bestilt en utbetaling denne dagen. Kanseller den forrige og bestill på nytt.");
        }

        var futureFlexEntries =
            (await _timeRegistrationService.GetFlexTimeEntries()).Where(entry => entry.Date > DateTime.Now);

        if (futureFlexEntries.Any(e => e.Hours > 0))
        {
            throw new ValidationException("Fjern fremtidig avspasering før du gjør en bestilling.");
        }

        if (request.Hours > availableForPayout)
        {
            throw new ValidationException("Ikke nok tilgjengelige timer.");
        }

        await CheckForIncompleteDays(request, userId);
    }

    public virtual async Task CheckForIncompleteDays(GenericHourEntry request, int userId)
    {
        var entriesPast30Days = (await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = request.Date.AddDays(-30),
            ToDateInclusive = request.Date
        })).GroupBy(e => e.Date).ToList();
        var datesRegistered = entriesPast30Days.Select(e => e.Key.Date).ToList();

        var redDaysDates = new List<DateTime>();
        var redDaysInYear = new RedDays(request.Date.Year);
        var redDaysPreviousYear = new RedDays(request.Date.Year - 1);
        redDaysDates.AddRange(redDaysInYear.Dates);
        redDaysDates.AddRange(redDaysPreviousYear.Dates);
        
        var incompleteDays = new List<DateTime>();

        for (var i = 0; i < 30; i++)
        {
            var date = request.Date.AddDays(-i).Date;

            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || redDaysDates.Contains(date))
            {
                continue;
            }

            if (!datesRegistered.Contains(date))
            {
                incompleteDays.Add(date);
                continue;
            }

            var recordedDay = entriesPast30Days.First(e => e.Key.Date == date);
            var employmentRateOnDay =
                await _userService.GetCurrentEmploymentRateForUser(userId, date);
            var anticipatedWorkHours = HoursInWorkDay * employmentRateOnDay;
            if (recordedDay.Sum(d => d.Value) < anticipatedWorkHours)
            {
                incompleteDays.Add(date);
            }
        }

        if (incompleteDays.Any())
        {
            throw new ValidationException(
                $"Du har ikke fylt opp følgende dager: {string.Join(", ", incompleteDays.Select(i => i.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))}");
        }
    }

    public async Task ValidatePayoutCancellation(DateTime payoutDate, int userId)
    {
        var allActivePayouts = await _payoutStorage.GetActivePayouts(userId);

        if (!allActivePayouts.Any())
        {
            throw new ValidationException("Det er ingen aktive utbetalinger.");
        }

        var payoutsToBeCancelled =
            (await _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter { UserId = userId })).Entries.Where(po => po.Date.Date == payoutDate).ToList();

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