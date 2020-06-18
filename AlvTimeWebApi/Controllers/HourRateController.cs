using AlvTime.Business.HourRates;
using AlvTime.Business.Tasks;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.HourRates
{
    [Route("api/admin")]
    [ApiController]
    public class HourRateController : Controller
    {
        private readonly IHourRateStorage _storage;
        private readonly HourRateCreator _creator;

        public HourRateController(IHourRateStorage storage, HourRateCreator creator)
        {
            _storage = storage;
            _creator = creator;
        }

        [HttpGet("HourRates")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> FetchHourRates()
        {
            return Ok(_storage.GetHourRates(new HourRateQuerySearch()));
        }

        [HttpPost("HourRates")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> CreateHourRate([FromBody] IEnumerable<CreateHourRateDto> hourRatesToBeCreated)
        {
            List<HourRateResponseDto> response = new List<HourRateResponseDto>();

            foreach (var hourRate in hourRatesToBeCreated)
            {
                response.Add(_creator.CreateHourRate(hourRate));
            }

            return Ok(response);
        }
    }
}
