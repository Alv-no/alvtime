using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.HourRates
{
    public interface IHourRateStorage
    {
        IEnumerable<HourRateResponseDto> GetHourRates(HourRateQuerySearch criterias);
        void CreateHourRate(CreateHourRateDto hourRate);
        void UpdateHourRate(CreateHourRateDto hourRate);
    }

    public class HourRateQuerySearch
    {
        public DateTime? FromDate { get; set; }
        public decimal? Rate { get; set; }
        public int? TaskId { get; set; }
    }
}
