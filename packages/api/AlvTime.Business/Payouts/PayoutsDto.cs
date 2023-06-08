﻿using System.Collections.Generic;

namespace AlvTime.Business.Payouts;

public class PayoutsDto
{
    public decimal TotalHoursBeforeCompRate { get; set; }
    public decimal TotalHoursAfterCompRate { get; set; }
    public List<GenericPayoutEntry> Entries { get; set; }
}