using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public ActionResult RegisterHourlySalary([FromBody] EmployeeSalaryRequest employeeSalaryData)
        {
            var salary = ToEmployeeSalaryRespons(_salaryService.RegisterHourlySalary(employeeSalaryData));
            return Ok(salary);
        }

        [AuthorizeAdmin]
        [HttpGet("/EmployeeSalary")]
        public ActionResult<List<EmployeeSalaryRespons>> GetEmployeeSalaryData(int userId)
        {
            var salaryData = _salaryService.GetEmployeeSalaryData(userId).Select(ToEmployeeSalaryRespons)
                .ToList();
            return Ok(salaryData);
        }

        private EmployeeSalaryRespons ToEmployeeSalaryRespons(EmployeeSalary employeeHourlySalary)
        {
            return new()
            {
                Id = employeeHourlySalary.Id,
                UsiderId = employeeHourlySalary.UsiderId,
                FromDate = employeeHourlySalary.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ToDate = employeeHourlySalary.ToDate.HasValue
                    ? employeeHourlySalary.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : null,
                HourlySalary = employeeHourlySalary.HourlySalary
            };
        }
    }
}