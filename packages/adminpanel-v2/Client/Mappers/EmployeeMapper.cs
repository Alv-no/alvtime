using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class EmployeeMapper
{
    public static EmployeeCreateRequest MapToEmployeeCreateRequest(this EmployeeModel employee)
    {
        return new EmployeeCreateRequest
        {
            Name = employee.Name,
            Email = employee.Email,
            StartDate = employee.StartDate,
            EndDate = employee.EndDate,
            EmployeeId = employee.EmployeeId
        };
    }

    public static EmployeeUpdateRequest MapToEmployeeUpdateRequest(this EmployeeModel employee)
    {
        return new EmployeeUpdateRequest
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            StartDate = employee.StartDate,
            EndDate = employee.EndDate,
            EmployeeId = employee.EmployeeId
        };
    }
}