using System;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Task = System.Threading.Tasks.Task;

namespace AlvTimeWebApi.Authorization.Handlers;

public class EmployeeIsActiveHandler : AuthorizationHandler<EmployeeStillActiveRequirement>
{
    private readonly AlvTime_dbContext _alvtimeDbContext;

    public EmployeeIsActiveHandler(AlvTime_dbContext alvtimeDbContext)
    {
        _alvtimeDbContext = alvtimeDbContext;
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext, EmployeeStillActiveRequirement requirement)
    {
        var oid = authContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        
        if (oid is null)
        {
            authContext.Fail(new AuthorizationFailureReason(this, "User oid not set in token"));
            return Task.CompletedTask;
        }

        var employee = _alvtimeDbContext.User.FirstOrDefault(u => u.Oid.Equals(oid));

        if (employee is null)
        {
            authContext.Fail(new AuthorizationFailureReason(this, "Employee not found"));
            return Task.CompletedTask;
        }

        if (employee.EndDate < DateTime.Now)
        {
            authContext.Fail(new AuthorizationFailureReason(this, "Employee is no longer active"));
            return Task.CompletedTask;
        }
        
        if (employee.StartDate > DateTime.Now)
        {
            authContext.Fail(new AuthorizationFailureReason(this, "Employee is not active yet"));
            return Task.CompletedTask;
        }

        authContext.Succeed(requirement);

        return Task.CompletedTask;
    }
}