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
        private readonly IFlexihourRepository _storage;
        private readonly RetrieveUsers _userRetriever;

        public FlexiHourController(RetrieveUsers userRetriever, IFlexihourRepository storage)
        {
            _storage = storage;
            _userRetriever = userRetriever;
        }

        [HttpGet("FlexiHours")]
        [Authorize]
        public ActionResult<IEnumerable<FlexiHoursResponseDto>> FetchFlexiHour(DateTime startDate, DateTime endDate)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage
                .GetFlexihours(startDate, endDate, user.Id)
                .Select(f => new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Result = new FlexiHoursResponseDto
                    {
                        Date = f.Date.ToDateOnly(),
                        Value = f.Value
                    }
                }));
        }
    }
}