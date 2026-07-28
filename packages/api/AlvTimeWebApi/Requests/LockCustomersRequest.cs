using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class LockCustomersRequest
{
    [Required]
    public DateTime ToDateInclusive { get; set; }
    public List<int> CustomersToExclude { get; set; } = [];
}
