using AlvTime.Business;
using AlvTime.Business.FlexiHours;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace AlvTimeWebApi.Controllers.FlexiHours
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

        [HttpGet("TotalFlexiHours")]
        [Authorize]
        public ActionResult<FlexiHourResponseDto> FetchTotalFlexiHours()
        {
            return Ok(_storage.GetTotalFlexiHours());
        }

        [HttpGet("UsedFlexiHours")]
        [Authorize]
        public ActionResult<FlexiHourResponseDto> FetchUsedFlexiHours()
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage.GetUsedFlexiHours(user.Id));
        }

        [HttpGet("FlexHours")]
        [Authorize]
        public ActionResult<FlexiHourResponseDto> GetFlexHours()
        {
            return Ok(1);
        }
    }
}