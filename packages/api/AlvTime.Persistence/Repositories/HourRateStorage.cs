using AlvTime.Business.HourRates;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories;

public class HourRateStorage : IHourRateStorage
{
    private readonly AlvTime_dbContext _context;

    public HourRateStorage(AlvTime_dbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateHourRate(HourRateDto hourRate, int taskId)
    {
        var newRate = new HourRate
        {
            FromDate = hourRate.FromDate,
            Rate = hourRate.Rate,
            TaskId = taskId
        };

        _context.HourRate.Add(newRate);
        await _context.SaveChangesAsync();
        return newRate.Id;
    }

    public async Task UpdateHourRate(HourRateDto hourRate)
    {
        var existingRate = await _context.HourRate
            .FirstOrDefaultAsync(hr => hr.Id == hourRate.Id);

        existingRate.Rate = hourRate.Rate;
        existingRate.FromDate = hourRate.FromDate;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<HourRateDto>> GetHourRates(HourRateQuerySearch criterias)
    {
        return await _context.HourRate
            .AsQueryable()
            .Filter(criterias)
            .Select(x => new HourRateDto
            {
                FromDate = x.FromDate,
                Id = x.Id,
                Rate = x.Rate
            })
            .ToListAsync();
    }
    
    public async Task DeleteHourRate(int hourRateId)
    {
        var rate = await _context.HourRate
            .FirstOrDefaultAsync(hr => hr.Id == hourRateId);

        _context.HourRate.Remove(rate);
        await _context.SaveChangesAsync();
    }
}