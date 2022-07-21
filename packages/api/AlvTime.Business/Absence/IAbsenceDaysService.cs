using System;

namespace AlvTime.Business.Absence;

public interface IAbsenceDaysService
{
    AbsenceDaysDto GetAbsenceDays(int userId, int year, DateTime? intervalStart);
    VacationDaysDTO GetAllTimeVacationOverview(int currentYear);
}