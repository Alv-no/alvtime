using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.InvoiceRate;
using AlvTimeWebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AlvTime.Business.InvoiceRate.InvoiceStatisticsDto;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class InvoiceRateController(InvoiceRateService invoiceRateService) : ControllerBase
{
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

        return await invoiceRateService.GetEmployeeInvoiceRateForPeriod(fromDate.Value.Date, toDate.Value.Date);
    }
    
    [HttpGet("InvoiceRateByMonth")]
    public async Task<List<InvoiceRateByMonthDto>> FetchUserInvoiceRatePastXMonths([FromQuery] int monthsToFetch = 6)
    {
        if (monthsToFetch > 12)
        {
            monthsToFetch = 12;
        }
        var response = await invoiceRateService.GetEmployeeInvoiceRatePastXMonths(monthsToFetch);
        return response;
    }

    [HttpGet("InvoiceStatistics")]
    public async Task<InvoiceStatisticsDto> FetchUserInvoiceStatistics(DateTime fromDate, DateTime toDate, 
        InvoicePeriods period = InvoicePeriods.Monthly, ExtendPeriod extendPeriod = ExtendPeriod.None, bool includeZeroPeriods = false)
    {
        return await invoiceRateService.GetEmployeeInvoiceStatisticsByPeriod(fromDate.Date, toDate.Date, period, extendPeriod, includeZeroPeriods);
    }
}
