using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AlvTime.Business.CompensationRate
{
    public interface ICompensationRateStorage
    {
        Task<IEnumerable<CompensationRateSearchResultDto>> GetCompensationRates(CompensationRateQuerySearch criterias);
        Task CreateCompensationRate(CompensationRateDto compensationRateDto);
        Task<CompensationRateDto> UpdateCompensationRate();
    }

    public class CompensationRateQuerySearch
    {
        public int? Id { get; set; }
        public DateTime FromDate { get; set; }
        public decimal? Value{ get; set; }
        public int? TaskId { get; set; }
    }
}
