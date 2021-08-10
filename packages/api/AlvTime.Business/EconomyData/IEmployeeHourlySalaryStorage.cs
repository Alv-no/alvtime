using System;

namespace AlvTime.Business.EconomyData
{
    public interface IEmployeeHourlySalaryStorage
    {
        decimal GetHouerlySalary(int userId, DateTime date);
    }
}
