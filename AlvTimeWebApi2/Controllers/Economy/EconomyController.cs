using AlvTimeWebApi2.DataBaseModels;
using AlvTimeWebApi2.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi2.Controllers.Economy
{
    [Route("api/admin")]
    [ApiController]
    public class EconomyController : Controller
    {
        private readonly AlvTimeDBContext _database;

        public EconomyController(AlvTimeDBContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Retrieves info about customer and users
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpGet("EconomyInfo")]
        //[Authorize]
        public ActionResult<IEnumerable<EconomyDto>> FetchEconomyInfo()
        {
            var info = _database.VDataDump
                .Select(x => new EconomyDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    Date = x.Date,
                    Email = x.Email,
                    HourRate = x.HourRate,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,
                    TaskId = x.TaskId,
                    TaskName = x.TaskName,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Value = x.Value
                })
                .ToList();
            return Ok(info);
        }
    }
}
