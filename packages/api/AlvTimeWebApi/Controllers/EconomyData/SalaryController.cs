using System.Collections.Generic;
using AlvTime.Business.EconomyData;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers.EconomyData
{
    [Route("api/EconomyData")]
    [ApiController]
    public class SalaryController : Controller
    {
        private readonly ISalaryService _salaryService;

        public SalaryController(ISalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        [AuthorizeAdmin]
        [HttpPost("/EmployeeSalary")]
        public ActionResult RegisterHourlySalary([FromBody] EmployeeSalaryDto employeeSalaryData)
        {
            _salaryService.RegisterHourlySalary(employeeSalaryData);
            return Ok();
        }

        [AuthorizeAdmin]
        [HttpGet("/EmployeeSalary")]
        public ActionResult<List<EmployeeSalaryDto>> GetEmployeeSalaryData(int userId)
        {
            return Ok(_salaryService.GetEmployeeSalaryData(userId));
        }
    }
}