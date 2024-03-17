using System;
using AlvTime.Business.HourRates;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class HourRateController : Controller
{
    private readonly IHourRateStorage _hourRateStorage;
    private readonly HourRateService _hourRateService;

    public HourRateController(IHourRateStorage hourRateStorage, HourRateService hourRateService)
    {
        _hourRateStorage = hourRateStorage;
        _hourRateService = hourRateService;
    }

    [HttpPost("HourRates")]
    public async Task<ActionResult<IEnumerable<HourRateResponseDto>>> CreateHourRate([FromBody] IEnumerable<CreateHourRateDto> hourRatesToBeCreated)
    {
        List<HourRateResponseDto> response = new List<HourRateResponseDto>();

        foreach (var hourRate in hourRatesToBeCreated)
        {
            response.Add(await _hourRateService.CreateHourRate(hourRate));
        }

        return Ok(response);
    }
}