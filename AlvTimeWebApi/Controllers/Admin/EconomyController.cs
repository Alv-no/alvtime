using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Economy
{
    [Route("api/admin")]
    [ApiController]
    public class EconomyController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public EconomyController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("EconomyInfo")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<DataDumpDto>> FetchEconomyInfo()
        {
            var info = _database.VDataDump
                .Select(x => new DataDumpDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    Date = x.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Email = x.Email,
                    HourRate = x.HourRate,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,
                    TaskId = x.TaskId,
                    TaskName = x.TaskName,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Value = x.Value,
                    Earnings = x.Earnings,
                    IsBillable = x.IsBillable
                })
                .ToList();
            return Ok(info);
        }
    }
}
