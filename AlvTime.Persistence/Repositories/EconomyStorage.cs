using AlvTime.Business.Economy;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.Economy.EconomyStorage
{
    public class EconomyStorage : IEconomyStorage
    {
        private readonly AlvTime_dbContext _context;

        public EconomyStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public IEnumerable<DataDumpDto> GetEconomyInfo()
        {
            return _context.VDataDump
                .Select(x => new DataDumpDto
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
                .ToList();
        }
    }
}
