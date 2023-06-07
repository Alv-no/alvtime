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

    public async Task<HourRateResponseDto> CreateHourRate(CreateHourRateDto hourRate)
    {
        var hourRateExists = (await GetHourRate(hourRate)).Any();
        if (!hourRateExists)
            await _hourRateStorage.CreateHourRate(hourRate);
        else
            await _hourRateStorage.UpdateHourRate(hourRate);

        return (await GetHourRate(hourRate)).Single();
    }

    private async Task<IEnumerable<HourRateResponseDto>> GetHourRate(CreateHourRateDto hourRate)
    {
        return await _hourRateStorage.GetHourRates(new HourRateQuerySearch
        {
            FromDate = hourRate.FromDate,
            TaskId = hourRate.TaskId
        });
    }
}