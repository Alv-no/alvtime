using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Absence;

public interface IAbsenceDaysService
{
    Task<AbsenceDaysDto> GetAbsenceDays(int userId, DateTime? intervalStart);
    Task<VacationDaysDto> GetAllTimeVacationOverview(int currentYear);
    Task<List<VacationOverviewReport>> GetVacationOverviewForAllUsers(int currentYear);
}