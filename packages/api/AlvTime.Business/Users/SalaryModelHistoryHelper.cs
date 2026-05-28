using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Overtime;

namespace AlvTime.Business.Users;

public static class SalaryModelHistoryHelper
{
    public static SalaryModel GetModelAtDate(DateTime date, IReadOnlyList<SalaryModelHistoryEntry> history)
    {
        if (!history.Any()) return SalaryModel.Static;
        var lastSwitch = history.LastOrDefault(h => h.SwitchDate.Date <= date);
        return lastSwitch?.NewModel ?? history[0].PreviousModel;
    }
}
