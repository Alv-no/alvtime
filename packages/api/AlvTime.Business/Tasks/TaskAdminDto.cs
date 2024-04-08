using System.Collections.Generic;
using AlvTime.Business.CompensationRate;
using AlvTime.Business.HourRates;

namespace AlvTime.Business.Tasks;

public class TaskAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Locked { get; set; }
    public bool Imposed { get; set; }
    public decimal CompensationRate { get; set; }
    public IEnumerable<HourRateDto> HourRates { get; set; }
}