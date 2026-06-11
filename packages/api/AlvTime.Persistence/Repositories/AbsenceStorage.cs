using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Absence;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace AlvTime.Persistence.Repositories;

public class AbsenceStorage(AlvTime_dbContext context) : IAbsenceStorage
{
    public async Task<IEnumerable<CustomVacationOverrideOverview>> GetCustomVacationEarned(int userId)
    {
        return (await context.VacationDaysEarnedOverride.Where(v => v.UserId == userId).ToListAsync()).Select(v => new CustomVacationOverrideOverview
        {
            UserId = v.UserId,
            Year = v.Year,
            DaysEarned = v.DaysEarned
        });
    }

    public async Task<IEnumerable<CustomVacationOverrideOverview>> GetAllCustomVacationEarned()
    {
        return (await context.VacationDaysEarnedOverride.ToListAsync()).Select(v => new CustomVacationOverrideOverview
        {
            UserId = v.UserId,
            Year = v.Year,
            DaysEarned = v.DaysEarned
        });
    }
}