using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.CompensationRate
{
    public interface ICompensationRateStorage
    {
        IEnumerable<CompensationRateSearchResultDto> GetCompensationRates(CompensationRateQuerySearch criterias);
        void CreateCompensationRate(CompensationRateDto compensationRateDto);
        CompensationRateDto UpdateCompensationRate();
    }

    public class CompensationRateQuerySearch
    {
        public int? Id { get; set; }
        public DateTime FromDate { get; set; }
        public decimal? Value{ get; set; }
        public int? TaskId { get; set; }
    }
}
