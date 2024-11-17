using System;
using System.Threading.Tasks;
using AlvTime.Business.InvoiceRate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AlvTime.Business.InvoiceRate.InvoiceStatisticsDto;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class InvoiceRateController : ControllerBase
{

    private readonly InvoiceRateService _invoiceRateService;

    public InvoiceRateController(InvoiceRateService invoiceRateService)
    {
        _invoiceRateService = invoiceRateService;
    }

    [HttpGet("InvoiceRate")]
    public async Task<decimal> FetchUserInvoiceRate(DateTime? fromDate, DateTime? toDate)
    {
        if (!toDate.HasValue)
        {
            toDate = DateTime.Now;
        }

        if (!fromDate.HasValue)
        {
            var now = DateTime.Now;
            fromDate = new DateTime(now.Year, now.Month, 1);
        }

        return await _invoiceRateService.GetEmployeeInvoiceRateForPeriod(fromDate.Value.Date, toDate.Value.Date);
    }

    [HttpGet("InvoiceStatistics")]
    public async Task<InvoiceStatisticsDto> FetchUserInvoiceStatistics(DateTime fromDate, DateTime toDate, 
        InvoicePeriods period = InvoicePeriods.Monthly, ExtendPeriod extendPeriod = ExtendPeriod.None, bool includeZeroPeriods = false)
    {
        return await _invoiceRateService.GetEmployeeInvoiceStatisticsByPeriod(fromDate.Date, toDate.Date, period, extendPeriod, includeZeroPeriods);
    }
}
