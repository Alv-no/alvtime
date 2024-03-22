namespace Alvtime.Adminpanel.Client.Utils;

public static class ApiRoutes
{
    public static string UsersBase => "api/admin/users";
    public static string EmploymentRateForUser(int id) => $"{UsersBase}/{id}/employmentrates";
    public static string CustomersBase => "api/admin/customers";
}