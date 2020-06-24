using AlvTime.Business.Economy;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class EconomyController : Controller
    {
        private readonly IEconomyStorage _storage;

        public EconomyController(IEconomyStorage storage)
        {
            _storage = storage;
        }

        [HttpGet("EconomyInfo")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<DataDumpDto>> FetchEconomyInfo()
        {
            return Ok(_storage.GetEconomyInfo());
        }
    }
}
