using AlvTime.Business.HourRates;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.HourRates
{
    public class HourRateStorageTests
    {
        [Fact]
        public void GetHourRates_NoCriterias_AllHourRates()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new HourRateStorage(context);

            var hourRates = storage.GetHourRates(new HourRateQuerySearch());

            Assert.Equal(context.HourRate.Count(), hourRates.Count());
        }

        [Fact]
        public void GetHourRates_RateSpecified_AllHourRatesWithSpecifiedRate()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new HourRateStorage(context);

            var hourRates = storage.GetHourRates(new HourRateQuerySearch 
            {
                Rate = 1000
            });

            var contextHourRatesWithRate = context.HourRate
                .Where(hr => hr.Rate == 1000)
                .ToList();

            Assert.Equal(contextHourRatesWithRate.Count(), hourRates.Count());
        }

        [Fact]
        public void CreateHourRate_NewHourRate_HourRateIsCreated()
        {
            var context = new AlvTimeDbContextBuilder().WithCustomers().WithProjects().WithTasks().CreateDbContext();

            var storage = new HourRateStorage(context);
            var creator = new HourRateCreator(storage);

            var previousCountOfHourRates = context.HourRate.Count();

            creator.CreateHourRate(new CreateHourRateDto
            {
                FromDate = new DateTime(2019, 05, 01),
                Rate = 500,
                TaskId = 2
            });

            var newCountOfHourRates = context.HourRate.Count();

            Assert.Equal(previousCountOfHourRates+1, newCountOfHourRates);
        }

        [Fact]
        public void CreateHourRate_HourRateExists_RateIsUpdated()
        {
            var context = new AlvTimeDbContextBuilder().WithCustomers().WithProjects().WithTasks().CreateDbContext();

            var storage = new HourRateStorage(context);
            var creator = new HourRateCreator(storage);

            creator.CreateHourRate(new CreateHourRateDto
            {
                FromDate = new DateTime(2019, 01, 01),
                Rate = 800,
                TaskId = 1
            });

            var hourRate = context.HourRate
                .Where(hr => hr.FromDate == new DateTime(2019, 01, 01) && hr.TaskId == 1)
                .FirstOrDefault();

            Assert.Equal(800, hourRate.Rate);
        }
    }
}
