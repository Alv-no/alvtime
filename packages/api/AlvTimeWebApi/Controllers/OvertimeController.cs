using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class OvertimeController : ControllerBase
    {
        private readonly TimeRegistrationService _timeRegistrationService;

        public OvertimeController(TimeRegistrationService timeRegistrationService)
        {
            _timeRegistrationService = timeRegistrationService;
        }
        
        [HttpGet("AvailableHours")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public AvailableHoursDto FetchAvailableHours()
        {
            return _timeRegistrationService.GetAvailableOvertimeHours();
        }
        
        [HttpGet("EarnedOvertime")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public List<EarnedOvertimeDto> FetchEarnedOvertime(DateTime startDate, DateTime endDate)
        {
            return _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter { StartDate = startDate, EndDate = endDate});
        }
    }
}