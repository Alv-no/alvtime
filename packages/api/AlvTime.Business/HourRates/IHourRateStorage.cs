using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.HourRates;

public interface IHourRateStorage
{
    Task<IEnumerable<HourRateDto>> GetHourRates(HourRateQuerySearch criterias);
    Task<int> CreateHourRate(HourRateDto hourRate, int taskId);
    Task UpdateHourRate(HourRateDto hourRate);
    Task DeleteHourRate(int hourRateId);
}

public class HourRateQuerySearch
{
    public int? Id { get; set; }
    public DateTime? FromDate { get; set; }
    public decimal? Rate { get; set; }
    public int? TaskId { get; set; }
}