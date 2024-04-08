using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.HourRates;

public class HourRateService
{
    private readonly IHourRateStorage _hourRateStorage;

    public HourRateService(IHourRateStorage hourRateStorage)
    {
        _hourRateStorage = hourRateStorage;
    }

    public async Task<HourRateDto> CreateHourRate(HourRateDto hourRate, int taskId)
    {
        var createdRateId = await _hourRateStorage.CreateHourRate(hourRate, taskId);
        return await GetHourRateById(createdRateId);
    }

    public async Task<HourRateDto> UpdateHourRate(HourRateDto hourRate)
    {
        await _hourRateStorage.UpdateHourRate(hourRate);
        return await GetHourRateById(hourRate.Id);
    }

    private async Task<HourRateDto> GetHourRateById(int hourRateId)
    {
        return (await _hourRateStorage.GetHourRates(new HourRateQuerySearch
        {
            Id = hourRateId
        })).First();
    }
}