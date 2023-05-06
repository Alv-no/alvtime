using AlvTime.Business.Economy;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [AuthorizeAdmin]
    public class EconomyController : Controller
    {
        private readonly IEconomyStorage _economyStorage;

        public EconomyController(IEconomyStorage economyStorage)
        {
            _economyStorage = economyStorage;
        }

        [HttpGet("EconomyInfo")]
        public async Task<ActionResult<IEnumerable<DataDumpDto>>> FetchEconomyInfo()
        {
            return Ok(await _economyStorage.GetEconomyInfo());
        }
    }
}
