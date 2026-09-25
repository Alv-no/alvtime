using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Policy = "AdminPolicy")]
public class OvertimeController(TimeRegistrationService timeRegistrationService) : ControllerBase
{
    [HttpGet("EarnedOvertime")]
    public async Task<List<EarnedOvertimeDto>> FetchEarnedOvertimeForAllUsers(DateTime? startDate, DateTime? endDate)
    {
        return await timeRegistrationService.GetEarnedOvertimeForAllUsers(new OvertimeQueryFilter
            { FromDateInclusive = startDate, ToDateInclusive = endDate });
    }

    [HttpGet("RegisteredFlex")]
    public async Task<List<RegisteredFlexDto>> FetchRegisteredFlexForAllUsers(DateTime? startDate, DateTime? endDate)
    {
        return await timeRegistrationService.GetRegisteredFlexForAllUsers(new OvertimeQueryFilter
            { FromDateInclusive = startDate, ToDateInclusive = endDate });
    }
}
