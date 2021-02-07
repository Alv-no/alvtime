using AlvTime.Business;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class HolidayController : Controller
    {
        [HttpGet("Holidays")]
        [Authorize]
        public ActionResult<IEnumerable<string>> FetchRedDays([FromQuery] int year)
        {
            var redDays = new RedDays(year);
            var dates = new List<string>();

            foreach (var date in redDays.Dates)
            {
                dates.Add(date.ToDateOnly());
            }

            return Ok(dates);
        }
    }
}