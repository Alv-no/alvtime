namespace AlvTime.Business.EconomyData
{
    public record EmployeeSalaryResponse (int Id, int UserId, decimal HourlySalary, string FromDate, string ToDate)
    {
    }
}
