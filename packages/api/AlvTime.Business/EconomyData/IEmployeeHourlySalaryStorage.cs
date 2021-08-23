﻿using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.EconomyData
{
    public interface IEmployeeHourlySalaryStorage
    {
        decimal GetHourlySalary(int userId, DateTime date);
        void RegisterHourlySalary(EmployeeSalaryDto employeeSalaryData);
        List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId);
        decimal CalculateOvertimeSalaryPayout(List<OvertimeEntry> overtimeEntriesForPayout, int userId);
    }
}