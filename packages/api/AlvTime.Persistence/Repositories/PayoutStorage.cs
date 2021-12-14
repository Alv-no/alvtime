using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Payouts;
using AlvTime.Persistence.DataBaseModels;
using FluentValidation;

namespace AlvTime.Persistence.Repositories
{
    public class PayoutStorage : IPayoutStorage
    {
        private readonly AlvTime_dbContext _context;

        public PayoutStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public PayoutsDto GetRegisteredPayouts(int userId)
        {
            var payouts = _context.PaidOvertime.Where(ot => ot.User == userId).ToList();

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
                    Active = po.Date.Month >= DateTime.Now.Month && po.Date.Year == DateTime.Now.Year
                }).ToList()
            };
        }

        public PayoutDto RegisterPayout(int userId, GenericHourEntry request, decimal payoutHoursAfterCompRate)
        {
            PaidOvertime paidOvertime = new PaidOvertime
            {
                Date = request.Date,
                User = userId,
                HoursBeforeCompRate = request.Hours,
                HoursAfterCompRate = payoutHoursAfterCompRate
            };

            _context.PaidOvertime.Add(paidOvertime);
            _context.SaveChanges();

            return new PayoutDto()
            {
                Id = paidOvertime.Id,
                UserId = paidOvertime.Id,
                Date = paidOvertime.Date,
                HoursBeforeCompensation = paidOvertime.HoursBeforeCompRate,
                HoursAfterCompensation = paidOvertime.HoursAfterCompRate
            };
        }

        public List<PayoutDto> GetActivePayouts(int userId)
        {
            var allActivePayouts = _context.PaidOvertime
                .Where(p => p.Date.Month >= DateTime.Now.Month &&
                            p.Date.Year == DateTime.Now.Year &&
                            p.User == userId).ToList();

            return allActivePayouts.Select(po => new PayoutDto
            {
                Date = po.Date,
                Id = po.Id,
                UserId = po.User,
                HoursAfterCompensation = po.HoursAfterCompRate,
                HoursBeforeCompensation = po.HoursBeforeCompRate
            }).ToList();
        }

        public PayoutDto CancelPayout(int payoutId)
        {
            var payout = _context.PaidOvertime.First(po => po.Id == payoutId);
            _context.PaidOvertime.Remove(payout);
            _context.SaveChanges();

            return new PayoutDto()
            {
                Date = payout.Date,
                Id = payout.Id,
                UserId = payout.User,
                HoursBeforeCompensation = payout.HoursBeforeCompRate,
                HoursAfterCompensation = payout.HoursAfterCompRate
            };
        }
    }
}