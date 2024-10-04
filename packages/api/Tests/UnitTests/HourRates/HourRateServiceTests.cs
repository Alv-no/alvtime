using AlvTime.Business.HourRates;
using System;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.HourRates;

public class HourRateServiceTests
{
    private readonly AlvTime_dbContext _context; 
    
    public HourRateServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithCustomers()
            .WithProjects()
            .WithTasks()
            .WithHourRates()
            .CreateDbContext();
    }

    [Fact]
    public async Task GetHourRate_WithId_ReturnsCorrectHourRate()
    {
        var hourRateService = CreateHourRateService(_context);
        var hourRate = await hourRateService.GetHourRateById(1);

        Assert.Equal(1, hourRate.Id);
    }

    [Fact]
    public async Task CreateHourRate_NewHourRate_HourRateIsCreated()
    {
        var hourRateService = CreateHourRateService(_context);

        var previousCountOfHourRates = (await hourRateService.GetHourRates(new HourRateQuerySearch())).Count();

        await hourRateService.CreateHourRate(new HourRateDto
        {
            FromDate = new DateTime(2019, 05, 01),
            Rate = 500
        }, 2);

        var newCountOfHourRates = (await hourRateService.GetHourRates(new HourRateQuerySearch())).Count();

        Assert.Equal(previousCountOfHourRates + 1, newCountOfHourRates);
    }

    [Fact]
    public async Task CreateHourRate_HourRateExists_RateIsUpdated()
    {
        var hourRateService = CreateHourRateService(_context);

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

        var hourRate = await hourRateService.GetHourRateById(hr.Id);

        Assert.Equal(900, hourRate.Rate);
    }

    private AlvTime.Business.HourRates.HourRateService CreateHourRateService(AlvTime_dbContext context)
    {
        var storage = new HourRateStorage(context);
        return new AlvTime.Business.HourRates.HourRateService(storage);
    }
}