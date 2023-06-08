using AlvTime.Business.Economy;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace AlvTime.Persistence.Repositories
{
    public class EconomyStorage : IEconomyStorage
    {
        private readonly AlvTime_dbContext _context;

        public EconomyStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EconomyInfoDto>> GetEconomyInfo()
        {
            return await _context.VDataDump
                .Select(x => new EconomyInfoDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    Date = x.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Email = x.Email,
                    HourRate = x.HourRate,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,
                    TaskId = x.TaskId,
                    TaskName = x.TaskName,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Value = x.Value,
                    Earnings = x.Earnings,
                    IsBillable = x.IsBillable
                })
                .ToListAsync();
        }
    }
}
