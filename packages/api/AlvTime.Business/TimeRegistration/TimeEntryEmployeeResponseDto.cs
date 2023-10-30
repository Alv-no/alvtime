using System;

namespace AlvTime.Business.TimeRegistration;

public class TimeEntryEmployeeResponseDto
{
    public int User { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
}