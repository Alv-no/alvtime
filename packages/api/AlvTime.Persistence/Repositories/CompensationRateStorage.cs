using AlvTime.Business.CompensationRate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories
{
    public class CompensationRateStorage : ICompensationRateStorage
    {
        private readonly AlvTime_dbContext _context;

        public CompensationRateStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public async Task CreateCompensationRate(CompensationRateDto compensationRateDto)
        {
            var compensationRate = new CompensationRate
            {
                FromDate = compensationRateDto.FromDate,
                TaskId = compensationRateDto.TaskId,
                Value = compensationRateDto.Value
            };

            _context.CompensationRate.Add(compensationRate);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CompensationRateSearchResultDto>> GetCompensationRates(CompensationRateQuerySearch criterias)
        {
            var compRates = await _context.CompensationRate.AsQueryable()
                .Filter(criterias)
                .Select(x => new CompensationRateSearchResultDto
                {
                    Id = x.Id,
                    FromDate = x.FromDate,
                    Value = x.Value,
                    TaskId = x.TaskId
                }).ToListAsync();

            return compRates;
        }

        public Task<CompensationRateDto> UpdateCompensationRate()
        {
            throw new NotImplementedException();
        }
    }
}
