using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Overtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class OvertimeController : ControllerBase
    {
        private readonly OvertimeService _overtimeService;

        public OvertimeController(OvertimeService overtimeService)
        {
            _overtimeService = overtimeService;
        }
        
        [HttpGet("AvailableHours")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public AvailableHoursDto FetchAvailableHours()
        {
            return _overtimeService.GetAvailableOvertimeHours();
        }
        
        [HttpGet("EarnedOvertime")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public List<EarnedOvertimeDto> FetchEarnedOvertime(DateTime startDate, DateTime endDate)
        {
            return _overtimeService.GetEarnedOvertime(new OvertimeQueryFilter { StartDate = startDate, EndDate = endDate});
        }
    }
}