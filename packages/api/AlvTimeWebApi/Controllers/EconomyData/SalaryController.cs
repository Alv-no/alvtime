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
        private readonly IEmployeeHourlySalaryStorage _employeeHourlySalaryStorage;

        public SalaryController(IEmployeeHourlySalaryStorage employeeHourlySalaryStorage)
        {
            _employeeHourlySalaryStorage = employeeHourlySalaryStorage;
        }

        [AuthorizeAdmin]
        [HttpPost("/EmployeeSalary")]
        public ActionResult RegisterHourlySalary([FromBody] EmployeeSalaryDto employeeSalaryData)
        {
            _employeeHourlySalaryStorage.RegisterHourlySalary(employeeSalaryData);
            return Ok();
        }

        [AuthorizeAdmin]
        [HttpGet("/EmployeeSalary")]
        public ActionResult<List<EmployeeSalaryDto>> GetEmployeeSalaryData(int userId)
        {
            return Ok(_employeeHourlySalaryStorage.GetEmployeeSalaryData(userId));
        }
    }
}