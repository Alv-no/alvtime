using System;
using System.Threading.Tasks;
using AlvTime.Business.InvoiceRate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
public class InvoiceRateController : ControllerBase
{

    private readonly InvoiceRateService _invoiceRateService;

    public InvoiceRateController(InvoiceRateService invoiceRateService)
    {
        _invoiceRateService = invoiceRateService;
    }

    [HttpGet("InvoiceRate")]
    [Authorize(Policy = "AllowPersonalAccessToken")]
    public async Task<decimal> FetchUserInvoiceRate(DateTime? fromDate, DateTime? toDate)
    {
        if (!toDate.HasValue)
        {
            toDate = DateTime.Now;
        }

        if (!fromDate.HasValue)
        {
            fromDate = DateTime.Now.AddDays(-30);
        }

        return await _invoiceRateService.GetEmployeeInvoiceRateForPeriod(fromDate.Value.Date, toDate.Value.Date);
    }
}
