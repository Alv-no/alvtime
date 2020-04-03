using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class HourRateController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public HourRateController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("HourRates")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> FetchHourRates()
        {
            var hourRates = _database.HourRate
                .Select(x => new HourRateResponseDto
                {
                    FromDate = x.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Id = x.Id,
                    Rate = x.Rate,
                    Task = _database.Task
                    .Where(y => y.Id == x.TaskId)
                    .Select(y => new TaskResponseDto
                    {
                        Description = y.Description,
                        Id = y.Id,
                        Favorite = false,
                        Locked = y.Locked,
                        Name = y.Name,
                        CompensationRate = y.CompensationRate,
                        Project = new ProjectResponseDto
                        {
                            Id = y.ProjectNavigation.Id,
                            Name = y.ProjectNavigation.Name,
                            Customer = new CustomerDto
                            {
                                Id = y.ProjectNavigation.CustomerNavigation.Id,
                                Name = y.ProjectNavigation.CustomerNavigation.Name,
                                InvoiceAddress = y.ProjectNavigation.CustomerNavigation.InvoiceAddress,
                                ContactPhone = y.ProjectNavigation.CustomerNavigation.ContactPhone,
                                ContactEmail = y.ProjectNavigation.CustomerNavigation.ContactEmail,
                                ContactPerson = y.ProjectNavigation.CustomerNavigation.ContactPerson
                            }
                        }
                    })
                    .FirstOrDefault()
                })
                .ToList();

            return Ok(hourRates);
        }

        [HttpPost("HourRates")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<HourRateResponseDto>> CreateHourRate([FromBody] IEnumerable<CreateHourRateDto> hourRatesToBeCreated)
        {
            List<HourRateResponseDto> response = new List<HourRateResponseDto>();

            foreach (var hourRate in hourRatesToBeCreated)
            {
                if (checkExisting.HourRateDoesNotExist(hourRate))
                {
                    var newHourRate = new HourRate
                    {
                        FromDate = hourRate.FromDate,
                        Rate = hourRate.Rate,
                        TaskId = hourRate.TaskId
                    };
                    _database.HourRate.Add(newHourRate);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCreatedHourRate(hourRate));
                }
            }
            return Ok(response);
        }
    }
}
