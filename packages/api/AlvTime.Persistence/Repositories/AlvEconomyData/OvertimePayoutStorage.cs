using System.Globalization;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.EconomyDataDBModels;
using FluentValidation;

namespace AlvTime.Persistence.Repositories.AlvEconomyData
{
    public class OvertimePayoutStorage : IOvertimePayoutStorage
    {
        private readonly AlvEconomyDataContext _economyContext;

        public OvertimePayoutStorage(AlvEconomyDataContext economyContext)
        {
            _economyContext = economyContext;
        }

        public OvertimePayoutRespons DeleteOvertimePayout(int userId, int paidOvertimeId)
        {
            var overtimePayout =
                _economyContext.OvertimePayouts.FirstOrDefault(op => op.UserId == userId && op.RegisteredPaidOvertimeId == paidOvertimeId);

            if (overtimePayout == null)
            {
                //Ida Therese, todo: denne må fjernes etterhvert som logikken for å beregne overtidsutbetalingen har vært i produksjon en stund da det pr nå ikke er noen overtidsutbetalinger å slette
                //throw new ValidationException("Could not find a payout registered for this user");
                return null;
            }

            _economyContext.OvertimePayouts.Remove(overtimePayout);
            _economyContext.SaveChanges();

            return new OvertimePayoutRespons (
                    overtimePayout.Id,
                    overtimePayout.UserId, 
                    overtimePayout.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    overtimePayout.TotalPayout, 
                    overtimePayout.RegisteredPaidOvertimeId);
        }

        public OvertimePayoutRespons SaveOvertimePayout(RegisterOvertimePayout overtimePayout)
        {
            var overtimePayoutEntity = new OvertimePayout
            {
                UserId = overtimePayout.UserId,
                Date = overtimePayout.Date,
                TotalPayout = overtimePayout.TotalPayout,
                RegisteredPaidOvertimeId = overtimePayout.PaidOvertimeId
            };

            _economyContext.OvertimePayouts.Add(overtimePayoutEntity);

            _economyContext.SaveChanges();
            return new OvertimePayoutRespons(
                overtimePayoutEntity.Id, 
                overtimePayoutEntity.UserId,
                overtimePayoutEntity.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                overtimePayoutEntity.TotalPayout, 
                overtimePayoutEntity.RegisteredPaidOvertimeId);
        }
    }
}