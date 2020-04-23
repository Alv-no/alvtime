using AlvTime.Business;
using AlvTimeWebApi.Dto;
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
        private readonly AlvTime_dbContext _database;
        private RetrieveUsers _userRetriever;

        public FlexiHourController(AlvTime_dbContext database, RetrieveUsers userRetriever)
        {
            _database = database;
            _userRetriever = userRetriever;
        }

        [HttpGet("TotalFlexiHours")]
        [Authorize]
        public ActionResult<FlexiHourResponseDto> FetchTotalFlexiHours()
        {
            var calculator = new AlvHoursCalculator();

            return new FlexiHourResponseDto
            {
                FlexiHours = 187.5M + calculator.CalculateAlvHours()
            };
        }

        [HttpGet("UsedFlexiHours")]
        [Authorize]
        public ActionResult<FlexiHourResponseDto> FetchUsedFlexiHours()
        {
            var user = _userRetriever.RetrieveUser();

            var currentYear = DateTime.UtcNow.Year;

            decimal totalUsedHours = 0;

            var hourList = _database.Hours
                .Where(x => x.User == user.Id && x.TaskId == 13 && x.Year == currentYear)
                .ToList();

            foreach (var timeEntry in hourList)
            {
                totalUsedHours += timeEntry.Value;
            }

            return new FlexiHourResponseDto
            {
                FlexiHours = totalUsedHours
            };
        }
    }
}