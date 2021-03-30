
using System;

namespace AlvTime.Business.AbsenseDays {
    public interface IAbsenseDaysService
    {
        AbsenseDaysDto GetAbsenseDays(int userId, int year, DateTime? intervalStart);
    }
}
