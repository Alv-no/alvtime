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
        public ActionResult<EmployeeSalaryResponse> RegisterHourlySalary([FromBody] EmployeeSalaryRequest employeeSalaryData)
        {
            var salary = ToEmployeeSalaryResponse(_salaryService.RegisterHourlySalary(employeeSalaryData));
            return Ok(salary);
        }

        [AuthorizeAdmin]
        [HttpGet("/EmployeeSalary")]
        public ActionResult<List<EmployeeSalaryResponse>> GetEmployeeSalaryData(int userId)
        {
            var salaryData = _salaryService.GetEmployeeSalaryData(userId).Select(ToEmployeeSalaryResponse)
                .ToList();
            return Ok(salaryData);
        }

        private EmployeeSalaryResponse ToEmployeeSalaryResponse(EmployeeSalaryDto employeeHourlySalary)
        {
            return new(employeeHourlySalary.Id, 
                employeeHourlySalary.UserId, 
                employeeHourlySalary.HourlySalary, 
                employeeHourlySalary.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                employeeHourlySalary.ToDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}