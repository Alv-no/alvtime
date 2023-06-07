using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.HourRates;

public interface IHourRateStorage
{
    Task<IEnumerable<HourRateResponseDto>> GetHourRates(HourRateQuerySearch criterias);
    Task CreateHourRate(CreateHourRateDto hourRate);
    Task UpdateHourRate(CreateHourRateDto hourRate);
}

public class HourRateQuerySearch
{
    public DateTime? FromDate { get; set; }
    public decimal? Rate { get; set; }
    public int? TaskId { get; set; }
}