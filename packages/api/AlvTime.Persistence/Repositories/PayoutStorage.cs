using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Payouts;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using User = AlvTime.Business.Users.User;

namespace AlvTime.Persistence.Repositories;

public class PayoutStorage : IPayoutStorage
{
    private readonly AlvTime_dbContext _context;

    public PayoutStorage(AlvTime_dbContext context)
    {
        _context = context;
    }

    public async Task<PayoutsDto> GetRegisteredPayouts(PayoutQueryFilter criterias)
    {
        var payouts = await _context.PaidOvertime.AsQueryable()
            .Filter(criterias)
            .ToListAsync();

        return new PayoutsDto
        {
            TotalHoursBeforeCompRate = payouts.Sum(po => po.HoursBeforeCompRate),
            TotalHoursAfterCompRate = payouts.Sum(po => po.HoursAfterCompRate),
            Entries = payouts.Select(po => new GenericPayoutEntry
            {
                Id = po.Id,
                Date = po.Date,
                HoursAfterCompRate = po.HoursAfterCompRate,
                HoursBeforeCompRate = po.HoursBeforeCompRate,
                Active = IsPayoutActive(po),
                CompRate = po.CompensationRate
            }).ToList()
        };
    }

    private static bool IsPayoutActive(PaidOvertime po)
    {
        //If payout made previous year and current day is more than 8, payout is inactive
        if (po.Date.Year < DateTime.Now.Year)
        {
            return po.Date.Month == 12 && po.Date.Day > 8 && DateTime.Now.DayOfYear <= 8;
        }

        //If payout is made after the 8th the previous month, or current date is before the 8th the succeeding month, payout is active
        if (po.Date.Month < DateTime.Now.Month)
        {
            if (po.Date.Day <= 8 || DateTime.Now.Day > 8)
            {
                return false;
            }

            if (po.Date.Day > 8 && DateTime.Now.Day <= 8 && DateTime.Now.Month == po.Date.Month + 1)
            {
                return true;
            }
        }

        //If payout made same month either after the 8th, or current date is before the 8th, payout is active
        return po.Date.Month == DateTime.Now.Month && (po.Date.Day > 8 || DateTime.Now.Day <= 8);
    }

    public async Task<List<PayoutDto>> RegisterPayout(int userId, GenericPayoutHourEntry request, List<PayoutToRegister> payoutsToRegister)
    {
        var response = new List<PayoutDto>();
        foreach (var payoutToRegister in payoutsToRegister)
        {
            PaidOvertime paidOvertime = new PaidOvertime
            {
                Date = request.Date,
                User = userId,
                HoursBeforeCompRate = payoutToRegister.HoursBeforeCompRate,
                HoursAfterCompRate = payoutToRegister.HoursAfterCompRate,
                CompensationRate = payoutToRegister.CompRate
            };

            _context.PaidOvertime.Add(paidOvertime);
                
            response.Add(new PayoutDto
            {
                Id = paidOvertime.Id,
                UserId = paidOvertime.User,
                Date = paidOvertime.Date,
                HoursBeforeCompensation = paidOvertime.HoursBeforeCompRate,
                HoursAfterCompensation = paidOvertime.HoursAfterCompRate
            });
        }

        await _context.SaveChangesAsync();

        return response;
    }

    public async Task<List<PayoutDto>> GetActivePayouts(int userId)
    {
        var allActivePayouts = await _context.PaidOvertime
            .Where(p => p.Date.Month >= DateTime.Now.Month &&
                        p.Date.Year == DateTime.Now.Year &&
                        p.User == userId).ToListAsync();

        return allActivePayouts.Select(po => new PayoutDto
        {
            Date = po.Date,
            Id = po.Id,
            UserId = po.User,
            HoursAfterCompensation = po.HoursAfterCompRate,
            HoursBeforeCompensation = po.HoursBeforeCompRate
        }).ToList();
    }

    public async Task CancelPayout(DateTime payoutDate, User currentUser)
    {
        var payouts = _context.PaidOvertime.Where(po => po.Date.Date == payoutDate && po.User == currentUser.Id);
        _context.PaidOvertime.RemoveRange(payouts);
        await _context.SaveChangesAsync();
    }
}