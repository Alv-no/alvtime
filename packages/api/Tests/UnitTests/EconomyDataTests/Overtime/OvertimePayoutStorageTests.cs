using System;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.EconomyDataTests.Overtime
{
    public class OvertimePayoutStorageTests
    {
        [Fact]
        public void AddTotalPayoutForOvertime_TotalPayoutIsAdded()
        {
            var context = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var storage = new OvertimePayoutStorage(context);
            var overtimePayout = new OvertimePayout
                {
                    UserId = 1, 
                    Date = new DateTime(day: 11, month: 08, year: 2021), 
                    TotalPayout = 1200.5M

                };
            
            storage.RegisterTotalOvertimePayout(new RegisterOvertimePayoutDto
            {
                Date = overtimePayout.Date, 
                UserId = overtimePayout.UserId, 
                TotalPayout = overtimePayout.TotalPayout
            });

            Assert.Equal(1, context.OvertimePayouts.FirstOrDefault(op => op.Id == 1).Id);
            Assert.Equal(overtimePayout.UserId, context.OvertimePayouts.FirstOrDefault(op => op.Id == 1).UserId);
            Assert.Equal(overtimePayout.Date, context.OvertimePayouts.FirstOrDefault(op => op.Id == 1).Date);
            Assert.Equal(overtimePayout.TotalPayout,
                context.OvertimePayouts.FirstOrDefault(op => op.Id == 1).TotalPayout);
        }

        [Fact]
        public void DeleteTotalPayoutForOvertime_TotalPayoutIsDeleted()
        {
            var context = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var storage = new OvertimePayoutStorage(context);

            var overTimePayoutForDeletion = new OvertimePayout
            {
                Date = new DateTime(day: 11, month: 08, year: 2021),
                UserId = 1, TotalPayout = 10.0M
            };

            context.OvertimePayouts.Add(overTimePayoutForDeletion);
            context.SaveChanges();

            var deletedOvertimePayout =
                storage.DeleteOvertimePayout(overTimePayoutForDeletion.UserId, overTimePayoutForDeletion.Date);

            Assert.Equal(overTimePayoutForDeletion.UserId, deletedOvertimePayout.UserId);
            Assert.Equal(overTimePayoutForDeletion.Date, deletedOvertimePayout.Date);
            Assert.Equal(overTimePayoutForDeletion.TotalPayout, deletedOvertimePayout.TotalPayout);
            Assert.Equal(1, deletedOvertimePayout.Id);
        }
    }
}