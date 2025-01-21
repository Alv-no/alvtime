using System;
using System.Collections.Generic;
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
    
    public async Task DeleteHourRate(int hourRateId)
    {
        await _hourRateStorage.DeleteHourRate(hourRateId);
    }
    
    public async Task<IEnumerable<HourRateDto>> GetHourRates(HourRateQuerySearch criterias)
    {
        return await _hourRateStorage.GetHourRates(criterias);
    }

    public async Task<HourRateDto> GetHourRateById(int hourRateId)
    {
        return (await _hourRateStorage.GetHourRates(new HourRateQuerySearch
        {
            Id = hourRateId
        })).First();
    }
}