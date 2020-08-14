using AlvTime.Business.FlexiHours;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class FlexiHourController : Controller
    {
        private readonly IFlexhourCalculator _storage;
        private readonly RetrieveUsers _userRetriever;

        public FlexiHourController(RetrieveUsers userRetriever, IFlexhourCalculator storage)
        {
            _storage = storage;
            _userRetriever = userRetriever;
        }

        [HttpGet("FlexiHours")]
        [Authorize]
        public ActionResult<IEnumerable<FlexiHoursResponseDto>> FetchFlexiHour(DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage
                .GetFlexihours(fromDateInclusive, toDateInclusive, user.Id)
                .Select(f => new
                {
                    Result = new FlexiHoursResponseDto
                    {
                        Date = f.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Value = f.Value
                    }
                }));
        }
    }
}