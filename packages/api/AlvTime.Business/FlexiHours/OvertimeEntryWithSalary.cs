namespace AlvTime.Business.FlexiHours
{
    public class OvertimeEntryWithSalary
    {
        public OvertimeEntryWithSalary(decimal salaryPrHour, OvertimeEntry overtimeEntry)
        {
            SalaryPrHour = salaryPrHour;
            OvertimeEntry = overtimeEntry;
        }
        public decimal SalaryPrHour { get;  }
        public OvertimeEntry OvertimeEntry { get; }
    }
}
