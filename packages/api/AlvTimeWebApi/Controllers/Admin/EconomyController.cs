using AlvTime.Business.Economy;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EconomyController : Controller
    {
        private readonly IEconomyStorage _economyStorage;

        public EconomyController(IEconomyStorage economyStorage)
        {
            _economyStorage = economyStorage;
        }

        [HttpGet("EconomyInfo")]
        public async Task<ActionResult<IEnumerable<EconomyInfoDto>>> FetchEconomyInfo()
        {
            return Ok(await _economyStorage.GetEconomyInfo());
        }
    }
}
