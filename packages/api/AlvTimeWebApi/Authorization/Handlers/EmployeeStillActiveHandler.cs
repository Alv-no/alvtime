using System;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Task = System.Threading.Tasks.Task;

namespace AlvTimeWebApi.Authorization.Handlers;

public class EmployeeStillActiveHandler : AuthorizationHandler<EmployeeStillActiveRequirement>
{
    private readonly AlvTime_dbContext _alvtimeContext;

    public EmployeeStillActiveHandler(AlvTime_dbContext alvtimeContext)
    {
        _alvtimeContext = alvtimeContext;
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployeeStillActiveRequirement requirement)
    {
        var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
        
        if (userEmail is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User email not set in token"));
            return Task.CompletedTask;
        }

        var employee = _alvtimeContext.User.FirstOrDefault(u => u.Email.ToLower().Equals(userEmail.ToLower()));

        if (employee is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "Employee not found"));
            return Task.CompletedTask;
        }

        if (employee.EndDate < DateTime.Now)
        {
            context.Fail(new AuthorizationFailureReason(this, "Employee is no longer active"));
        }
        else
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}