using System.Globalization;

namespace Alvtime.Adminpanel.Client.Utils;

public static class ApiRoutes
{
    public static string EmployeeBase => "api/admin/Users";
    public static string GetEmployeeById(int id) => $"{EmployeeBase}/{id}";
    public static string UpdateEmployee(int id) => $"{EmployeeBase}/{id}";
    public static string CreateEmploymentRateForUser(int id) => $"{EmployeeBase}/{id}/employmentrates";
    public static string UpdateEmploymentRateForUser(int id, int employmentRateId) => $"{EmployeeBase}/{id}/employmentrates/{employmentRateId}";
    public static string UpdateSalaryModel(int id) => $"{EmployeeBase}/{id}/salarymodel";
    public static string CancelPendingSalaryModelChange(int id) => $"{EmployeeBase}/{id}/salarymodel/pending";
    public static string CustomersBase => "api/admin/Customers";
    public static string ActiveCustomers => "api/admin/ActiveCustomers";
    public static string GetCustomerById(int id) => $"{CustomersBase}/{id}";
    public static string UpdateCustomer(int id) => $"{CustomersBase}/{id}";
    public static string LockAllCustomers => $"{CustomersBase}/Lock";
    public static string LockCustomer(int customerId, DateTime toDateInclusive) =>
        $"{CustomersBase}/Lock/{customerId}?toDateInclusive={IsoDate(toDateInclusive)}";
    public static string CreateProject(int customerId) => $"api/admin/Projects?customerId={customerId}";
    public static string UpdateProject(int projectId) => $"api/admin/Projects/{projectId}";
    public static string CreateTask(int projectId) => $"api/admin/Tasks?projectId={projectId}";
    public static string UpdateTask(int taskId) => $"api/admin/Tasks/{taskId}";
    public static string CreateHourRate(int taskId) => $"api/admin/HourRates?taskId={taskId}";
    public static string UpdateHourRate(int hourRateId) => $"api/admin/HourRates/{hourRateId}";
    public static string DeleteHourRate(int hourRateId) => $"api/admin/HourRates/{hourRateId}";
    public static string VacationOverview(int? year = null) => year.HasValue
        ? $"api/admin/vacationOverview?currentYear={year}"
        : "api/admin/vacationOverview";
    public static string VacationOverviewSingleUser(int userId, int? year = null) => year.HasValue
        ? $"api/admin/vacationOverview/{userId}?currentYear={year}"
        : $"api/admin/vacationOverview/{userId}";

    private static string IsoDate(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}