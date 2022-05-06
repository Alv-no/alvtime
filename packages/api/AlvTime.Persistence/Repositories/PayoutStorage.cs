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

        public PayoutsDto GetRegisteredPayouts(PayoutQueryFilter criterias)
        {
            var payouts = _context.PaidOvertime.AsQueryable()
                .Filter(criterias)
                .ToList();

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
                    Active = po.Date.Month >= DateTime.Now.Month && po.Date.Year == DateTime.Now.Year,
                    CompRate = po.CompensationRate
                }).ToList()
            };
        }

        public List<PayoutDto> RegisterPayout(int userId, GenericHourEntry request, List<PayoutToRegister> payoutsToRegister)
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

            _context.SaveChanges();

            return response;
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

        public void CancelPayout(DateTime payoutDate)
        {
            var payouts = _context.PaidOvertime.Where(po => po.Date.Date == payoutDate);
            _context.PaidOvertime.RemoveRange(payouts);
            _context.SaveChanges();
        }
    }
}