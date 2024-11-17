using AlvTime.Business.HourRates;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses.Admin;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class HourRateController : Controller
{
    private readonly HourRateService _hourRateService;

    public HourRateController(HourRateService hourRateService)
    {
        _hourRateService = hourRateService;
    }

    [HttpPost("HourRates")]
    public async Task<ActionResult<HourRateResponse>> CreateHourRate([FromBody] HourRateUpsertRequest hourRateToBeCreated, [FromQuery] int taskId)
    {
        var createdHourRate = await _hourRateService.CreateHourRate(hourRateToBeCreated.MapToHourRateDto(), taskId);
        return Ok(createdHourRate.MapToHourRateResponse());
    }

    [HttpPut("HourRates/{hourRateId:int}")]
    public async Task<ActionResult<HourRateResponse>> UpdateHourRate([FromBody] HourRateUpsertRequest hourRateToBeUpdated, int hourRateId)
    {
        var updatedRate = await _hourRateService.UpdateHourRate(hourRateToBeUpdated.MapToHourRateDto(hourRateId));
        return Ok(updatedRate.MapToHourRateResponse());
    }
}