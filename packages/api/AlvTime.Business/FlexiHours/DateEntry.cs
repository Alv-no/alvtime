using System;
using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours;

public class DateEntry
{
    public DateTime Date { get; set; }

    public IEnumerable<Entry> Entries { get; set; }
}