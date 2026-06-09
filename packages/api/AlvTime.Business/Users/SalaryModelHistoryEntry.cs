using System;
using AlvTime.Business.Overtime;

namespace AlvTime.Business.Users;

public record SalaryModelHistoryEntry(DateTime SwitchDate, SalaryModel PreviousModel, SalaryModel NewModel);
