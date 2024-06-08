using AlvTime.Business.HourRates;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.UnitTests.HourRates;

public class HourRateStorageTests
{
    [Fact]
    public async Task GetHourRates_NoCriterias_AllHourRates()
    {
        var context = new AlvTimeDbContextBuilder().CreateDbContext();

        var storage = new HourRateStorage(context);

        var hourRates = await storage.GetHourRates(new HourRateQuerySearch());

        Assert.Equal(context.HourRate.Count(), hourRates.Count());
    }

    [Fact]
    public async Task GetHourRates_RateSpecified_AllHourRatesWithSpecifiedRate()
    {
        var context = new AlvTimeDbContextBuilder().CreateDbContext();

        var storage = new HourRateStorage(context);

        var hourRates = await storage.GetHourRates(new HourRateQuerySearch
        {
            Rate = 1000
        });

        var contextHourRatesWithRate = context.HourRate
            .Where(hr => hr.Rate == 1000)
            .ToList();

        Assert.Equal(contextHourRatesWithRate.Count, hourRates.Count());
    }

    [Fact]
    public async Task CreateHourRate_NewHourRate_HourRateIsCreated()
    {
        var context = new AlvTimeDbContextBuilder().WithCustomers().WithProjects().WithTasks().CreateDbContext();

        var storage = new HourRateStorage(context);
        var creator = new HourRateService(storage);

        var previousCountOfHourRates = context.HourRate.Count();

        await creator.CreateHourRate(new HourRateDto
        {
            FromDate = new DateTime(2019, 05, 01),
            Rate = 500
        }, 2);

        var newCountOfHourRates = context.HourRate.Count();

        Assert.Equal(previousCountOfHourRates + 1, newCountOfHourRates);
    }

    [Fact]
    public async Task CreateHourRate_HourRateExists_RateIsUpdated()
    {
        var context = new AlvTimeDbContextBuilder().WithCustomers().WithProjects().WithTasks().CreateDbContext();

        var storage = new HourRateStorage(context);
        var hourRateService = new HourRateService(storage);

        var hr = await hourRateService.CreateHourRate(new HourRateDto
        {
            FromDate = new DateTime(2019, 01, 01),
            Rate = 800,
        }, 1);

        await hourRateService.UpdateHourRate(new HourRateDto
        {
            Id = hr.Id,
            FromDate = new DateTime(2020, 01, 01),
            Rate = 900
        });

        var hourRate = context.HourRate
            .FirstOrDefault(hr => hr.TaskId == 1);

        Assert.Equal(900, hourRate.Rate);
    }
}