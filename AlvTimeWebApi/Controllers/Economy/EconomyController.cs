using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Economy
{
    [Route("api/admin")]
    [ApiController]
    public class EconomyController : Controller
    {
        private readonly AlvTime_dbContext _database;

        public EconomyController(AlvTime_dbContext database)
        {
            _database = database;
        }

        [HttpGet("EconomyInfo")]
        [Authorize]
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
