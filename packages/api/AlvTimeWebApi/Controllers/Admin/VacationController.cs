using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Absence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AlvTimeWebApi.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[OutputCache(PolicyName = "Expire5Min")]
public class VacationController(IAbsenceDaysService absenceDaysService) : ControllerBase
{
    [HttpGet("vacationOverview")]
    public async Task<List<VacationOverviewReport>> FetchVacationOverview([FromQuery] int? currentYear)
    {
        if (!currentYear.HasValue)
        {
            currentYear = DateTime.Now.Year;
        }
        return await absenceDaysService.GetVacationOverviewForAllUsers(currentYear.Value);
    }
    
    [HttpGet("vacationOverview/{userId}")]
    public async Task<VacationOverviewReport> FetchVacationOverview([FromRoute] int userId, [FromQuery] int? currentYear)
    {
        if (!currentYear.HasValue)
        {
            currentYear = DateTime.Now.Year;
        }
        return await absenceDaysService.GetAllTimeVacationOverviewForSingleUser(currentYear.Value, userId);
    }
}