using System;

namespace AlvTime.Business.Customers;

public class CustomerResponseDto
{
    public string Name { get; set; }
    public DateTime? LockedTo { get; set; }
}