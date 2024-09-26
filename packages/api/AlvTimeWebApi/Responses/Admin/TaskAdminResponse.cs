using System.Collections.Generic;
using AlvTimeWebApi.Responses.Admin;

namespace AlvTimeWebApi.Responses;

public class TaskAdminResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Locked { get; set; }
    public bool Imposed { get; set; }
    public decimal CompensationRate { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public IEnumerable<HourRateAdminResponse> HourRates { get; set; }
}