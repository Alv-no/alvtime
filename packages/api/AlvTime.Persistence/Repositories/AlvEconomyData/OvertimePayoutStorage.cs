using System;
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

        public OvertimePayoutDto DeleteOvertimePayout(int userId, int paidOvertimeId)
        {
            var overtimePayout =
                _economyContext.OvertimePayouts.FirstOrDefault(op =>
                    op.UserId == userId && op.RegisteredPaidOvertimeId == paidOvertimeId);

            if (overtimePayout == null)
                //Todo: denne må fjernes etterhvert som logikken for å beregne overtidsutbetalingen har vært i produksjon en stund da det pr nå ikke er noen overtidsutbetalinger å slette
                //throw new ValidationException("Could not find a payout registered for this user");
                return null;

            using var alvEconomyDbTransaction = _economyContext.Database.BeginTransaction();
            try
            {
                _economyContext.OvertimePayouts.Remove(overtimePayout);
                _economyContext.SaveChanges();
                alvEconomyDbTransaction.Commit();

                return new OvertimePayoutDto(
                    overtimePayout.Id,
                    overtimePayout.UserId,
                    overtimePayout.Date,
                    overtimePayout.TotalPayout,
                    overtimePayout.RegisteredPaidOvertimeId);
            }
            catch (Exception e)
            {
                alvEconomyDbTransaction.Rollback();
                throw new ValidationException($"Unable to delete overtime payout for Paid overtime id: {paidOvertimeId}");
            }
        }

        public OvertimePayoutDto SaveOvertimePayout(RegisterOvertimePayout overtimePayout)
        {
            using var alvEconomyDbTransaction = _economyContext.Database.BeginTransaction();
            try
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
                alvEconomyDbTransaction.Commit();
                return new OvertimePayoutDto(
                    overtimePayoutEntity.Id,
                    overtimePayoutEntity.UserId,
                    overtimePayoutEntity.Date,
                    overtimePayoutEntity.TotalPayout,
                    overtimePayoutEntity.RegisteredPaidOvertimeId);
            }
            catch (Exception e)
            {
                alvEconomyDbTransaction.Rollback();
                throw new ValidationException($"Unable to save overtime payout for user: {overtimePayout.UserId}");
            }
        }
    }
}