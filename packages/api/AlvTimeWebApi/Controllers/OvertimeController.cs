using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Responses;
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
        public AvailableOvertimeResponse FetchAvailableHours()
        {
            var availableOvertime = _timeRegistrationService.GetAvailableOvertimeHoursNow();
            return new AvailableOvertimeResponse
            {
                AvailableHoursAfterCompensation = availableOvertime.AvailableHoursAfterCompensation,
                AvailableHoursBeforeCompensation = availableOvertime.AvailableHoursBeforeCompensation,
                Entries = availableOvertime.Entries.Select(entry => new TimeEntryResponse
                {
                    Date = entry.Date.ToDateOnly(),
                    Hours = entry.Hours,
                    CompensationRate = entry.CompensationRate
                }).ToList()
            };
        }
        
        [HttpGet("EarnedOvertime")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public List<EarnedOvertimeDto> FetchEarnedOvertime(DateTime startDate, DateTime endDate)
        {
            return _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter { StartDate = startDate, EndDate = endDate});
        }
    }
}