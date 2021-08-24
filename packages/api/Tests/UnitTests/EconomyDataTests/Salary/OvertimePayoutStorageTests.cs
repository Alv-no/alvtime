using System;
using System.Globalization;
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
                TotalPayout = 1200.5M,
                RegisteredPaidOvertimeId = 1
            };

            var savedOvertime = storage.SaveOvertimePayout(new RegisterOvertimePayoutDto
            {
                Date = overtimePayout.Date,
                UserId = overtimePayout.UserId,
                TotalPayout = overtimePayout.TotalPayout,
                PaidOvertimeId = overtimePayout.RegisteredPaidOvertimeId
            });

            Assert.Equal(1, savedOvertime.Id);
            Assert.Equal(overtimePayout.UserId, savedOvertime.UserId);
            Assert.Equal(overtimePayout.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), savedOvertime.Date);
            Assert.Equal(overtimePayout.TotalPayout, savedOvertime.TotalPayout);
            Assert.Equal(1, savedOvertime.PaidOvertimeId);
        }

        private OvertimePayoutStorage CreateStorage()
        {
            return new(_economyDataContext);
        }
    }
}