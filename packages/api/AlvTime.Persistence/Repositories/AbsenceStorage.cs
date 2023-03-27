using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Absence;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace AlvTime.Persistence.Repositories;

public class AbsenceStorage : IAbsenceStorage
{
    private readonly AlvTime_dbContext _context;

    public AbsenceStorage(AlvTime_dbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CustomVacationOverrideOverview>> GetCustomVacationEarned(int userId)
    {
        return (await _context.VacationDaysEarnedOverride.Where(v => v.UserId == userId).ToListAsync()).Select(v => new CustomVacationOverrideOverview
        {
            UserId = v.UserId,
            Year = v.Year,
            DaysEarned = v.DaysEarned
        });
    }
}