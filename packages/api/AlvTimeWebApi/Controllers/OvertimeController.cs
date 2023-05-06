using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    [AuthorizePersonalAccessToken]
    public class OvertimeController : ControllerBase
    {
        private readonly TimeRegistrationService _timeRegistrationService;

        public OvertimeController(TimeRegistrationService timeRegistrationService)
        {
            _timeRegistrationService = timeRegistrationService;
        }
        
        [HttpGet("AvailableHours")]
        public async Task<AvailableOvertimeResponse> FetchAvailableHours()
        {
            var availableOvertime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(9999, 01, 01));
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
        public async Task<List<EarnedOvertimeDto>> FetchEarnedOvertime(DateTime startDate, DateTime endDate)
        {
            return await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter { FromDateInclusive = startDate, ToDateInclusive = endDate});
        }
    }
}