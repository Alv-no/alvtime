using AlvTime.Business.HourRates;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class HourRateController : Controller
    {
        private readonly HourRateService _hourRateService;

        public HourRateController(HourRateService hourRateService)
        {
            _hourRateService = hourRateService;
        }

        [HttpGet("HourRates")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> FetchHourRates()
        {
            return Ok(_hourRateService.GetHourRates(new HourRateQuerySearch()));
        }

        [HttpPost("HourRates")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> CreateHourRate([FromBody] IEnumerable<CreateHourRateDto> hourRatesToBeCreated)
        {
            List<HourRateResponseDto> response = new List<HourRateResponseDto>();

            foreach (var hourRate in hourRatesToBeCreated)
            {
                response.Add(_hourRateService.CreateHourRate(hourRate));
            }

            return Ok(response);
        }
    }
}
