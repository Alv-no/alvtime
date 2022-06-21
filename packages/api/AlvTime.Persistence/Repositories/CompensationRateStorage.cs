using AlvTime.Business.CompensationRate;
using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public class CompensationRateStorage : ICompensationRateStorage
    {
        private readonly AlvTime_dbContext _context;

        public CompensationRateStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public void CreateCompensationRate(CompensationRateDto compensationRateDto)
        {
            var compensationRate = new CompensationRate
            {
                FromDate = compensationRateDto.FromDate,
                TaskId = compensationRateDto.TaskId,
                Value = compensationRateDto.Value
            };

            _context.CompensationRate.Add(compensationRate);
            _context.SaveChanges();
        }

        public IEnumerable<CompensationRateSearchResultDto> GetCompensationRates(CompensationRateQuerySearch criterias)
        {
            var compRates = _context.CompensationRate.AsQueryable()
                .Filter(criterias)
                .Select(x => new CompensationRateSearchResultDto
                {
                    Id = x.Id,
                    FromDate = x.FromDate,
                    Value = x.Value,
                    TaskId = x.TaskId
                }).ToList();

            return compRates;
        }

        public CompensationRateDto UpdateCompensationRate()
        {
            throw new NotImplementedException();
        }
    }
}
