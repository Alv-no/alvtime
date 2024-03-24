namespace Alvtime.Adminpanel.Client.Utils;

public static class ApiRoutes
{
    public static string EmployeeBase => "api/admin/users";
    public static string UpdateEmployee(int id) => $"{EmployeeBase}/{id}";
    public static string CreateEmploymentRateForUser(int id) => $"{EmployeeBase}/{id}/employmentrates";
    public static string UpdateEmploymentRateForUser(int id, int employmentRateId) => $"{EmployeeBase}/{id}/employmentrates/{employmentRateId}";
    public static string CustomersBase => "api/admin/customers";
    public static string UpdateCustomer(int id) => $"{CustomersBase}/{id}";
}