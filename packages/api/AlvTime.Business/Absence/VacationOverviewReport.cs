using System.Collections.Generic;

namespace AlvTime.Business.Absence;

public class VacationOverviewReport
{
    public int UserId { get; set; }
    public VacationDaysDto VacationDaysDto { get; set; }
}