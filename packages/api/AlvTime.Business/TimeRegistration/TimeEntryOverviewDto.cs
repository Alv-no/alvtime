using System.Collections.Generic;

namespace AlvTime.Business.TimeRegistration;

public class TimeEntryOverviewDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public IEnumerable<TaskWithHours> TasksWithHours { get; set; }
}

public class TaskWithHours
{
    public int TaskId { get; set; }
    public decimal Hours { get; set; }
}