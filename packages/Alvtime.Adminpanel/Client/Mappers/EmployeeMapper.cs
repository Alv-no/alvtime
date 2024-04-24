using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class EmployeeMapper
{
    public static EmployeeUpsertRequest MapToEmployeeUpsertRequest(this EmployeeModel employee)
    {
        return new EmployeeUpsertRequest
        {
            Name = employee.Name,
            Email = employee.Email,
            StartDate = employee.StartDate,
            EndDate = employee.EndDate,
            EmployeeId = employee.EmployeeId
        };
    }
}