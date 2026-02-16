using System;
using AlvTime.Business.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.TimeRegistration;

public class TimeEntryEmployeeResponseDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
}