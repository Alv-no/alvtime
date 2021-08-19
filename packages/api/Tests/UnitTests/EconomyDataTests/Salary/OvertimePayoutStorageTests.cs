using System;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.EconomyDataTests.Salary
{
    public class OvertimePayoutStorageTests
    {
        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();
        
        [Fact]
        public void SaveOvertimePayout_TotalPayoutIsAdded()
        {
            var storage = CreateStorage();
            var overtimePayout = new OvertimePayout
            {
                UserId = 1,
                Date = new DateTime(day: 11, month: 08, year: 2021),
                TotalPayout = 1200.5M
            };

            storage.SaveOvertimePayout(new RegisterOvertimePayoutDto
            {
                Date = overtimePayout.Date,
                UserId = overtimePayout.UserId,
                TotalPayout = overtimePayout.TotalPayout
            });

            Assert.Equal(1, _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).Id);
            Assert.Equal(overtimePayout.UserId,
                _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).UserId);
            Assert.Equal(overtimePayout.Date,
                _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).Date);
            Assert.Equal(overtimePayout.TotalPayout,
                _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).TotalPayout);
        }

        private OvertimePayoutStorage CreateStorage()
        {
            return new(_economyDataContext);
        }
    }
}