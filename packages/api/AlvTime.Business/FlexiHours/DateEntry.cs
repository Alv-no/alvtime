using System;
using System.Collections.Generic;
using System.Linq;

public class DateEntry
{
    public DateTime Date { get; set; }

    public IEnumerable<Entry> Entries { get; set; }

    public decimal GetWorkingHours()
    {
        return Entries.Sum(e => e.Value);
    }
}
