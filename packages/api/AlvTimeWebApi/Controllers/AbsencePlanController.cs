using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Absence;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AbsencePlanRequest = AlvTimeWebApi.Requests.AbsencePlanRequest;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class AbsencePlanController(AbsencePlanService absencePlanService) : ControllerBase
{
    [HttpGet("AbsencePlan/Tasks")]
    public async Task<ActionResult<List<AbsencePlanTaskResponse>>> FetchAbsencePlanTasks()
    {
        var tasks = await absencePlanService.GetAbsencePlanTasks();
        return Ok(tasks.Select(task => new AbsencePlanTaskResponse
        {
            Id = task.Id,
            Name = task.Name,
            IsPaidHoliday = task.IsPaidHoliday
        }).ToList());
    }

    [HttpGet("AbsencePlan")]
    public async Task<ActionResult<AbsencePlanResponse>> FetchAbsencePlan(
        [FromQuery] int taskId,
        [FromQuery] DateTime fromDateInclusive,
        [FromQuery] DateTime toDateInclusive,
        [FromQuery] decimal percentage = 100,
        [FromQuery] bool fillPartialDays = false)
    {
        var result = await absencePlanService.GetPlan(new AlvTime.Business.Absence.AbsencePlanRequest
        {
            TaskId = taskId,
            FromDateInclusive = fromDateInclusive,
            ToDateInclusive = toDateInclusive,
            Percentage = percentage,
            FillPartialDays = fillPartialDays
        });
        return result.Match<ActionResult<AbsencePlanResponse>>(
            plan => Ok(MapToResponse(plan)),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent fraværsplan feilet med følgende feil")));
    }

    [HttpPost("AbsencePlan")]
    public async Task<ActionResult<AbsencePlanResponse>> RegisterAbsencePlan([FromBody] AbsencePlanRequest request)
    {
        var result = await absencePlanService.RegisterPlan(new AlvTime.Business.Absence.AbsencePlanRequest
        {
            TaskId = request.TaskId,
            FromDateInclusive = request.FromDateInclusive,
            ToDateInclusive = request.ToDateInclusive,
            Percentage = request.Percentage,
            FillPartialDays = request.FillPartialDays,
            Comment = request.Comment
        });
        return result.Match<ActionResult<AbsencePlanResponse>>(
            plan => Ok(MapToResponse(plan)),
            errors => BadRequest(errors.ToValidationProblemDetails("Registrer fravær feilet med følgende feil")));
    }

    private static AbsencePlanResponse MapToResponse(AbsencePlanDto plan) => new()
    {
        TotalDays = plan.TotalDays,
        TotalHours = plan.TotalHours,
        Days = plan.Days.Select(day => new AbsencePlanDayResponse
        {
            Date = day.Date.ToDateOnly(),
            Status = day.Status.ToString(),
            Hours = day.Hours,
            ExistingOtherHours = day.ExistingOtherHours,
            Reason = day.Reason
        }).ToList()
    };
}
