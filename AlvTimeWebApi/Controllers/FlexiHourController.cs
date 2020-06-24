using AlvTime.Business.FlexiHours;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class FlexiHourController : Controller
    {
        private readonly IFlexiHourStorage _storage;
        private RetrieveUsers _userRetriever;

        public FlexiHourController(RetrieveUsers userRetriever, IFlexiHourStorage storage)
        {
            _storage = storage;
            _userRetriever = userRetriever;
        }

        [HttpGet("FlexiHours")]
        [Authorize]
        public ActionResult<FlexiHoursResponseDto> FetchFlexiHours(DateTime startDate, DateTime endDate)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage.GetFlexiHours(user.Id, startDate, endDate));
        }
    }
}