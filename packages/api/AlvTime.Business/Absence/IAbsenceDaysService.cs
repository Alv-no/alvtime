using System;
using System.Threading.Tasks;

namespace AlvTime.Business.Absence;

public interface IAbsenceDaysService
{
    Task<AbsenceDaysDto> GetAbsenceDays(int userId, int year, DateTime? intervalStart);
    Task<VacationDaysDTO> GetAllTimeVacationOverview(int currentYear);
}