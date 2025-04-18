﻿using System;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IUserRepository _userRepository;

    public UserController(UserService userService, IUserRepository userRepository)
    {
        _userService = userService;
        _userRepository = userRepository;
    }

    [HttpGet("Users")]
    public async Task<ActionResult<IEnumerable<UserAdminResponse>>> FetchUsers()
    {
        var result = await _userService.GetUsers(new UserQuerySearch());
        return result.Match<ActionResult<IEnumerable<UserAdminResponse>>>(
            users => Ok(users.Select(u => u.MapToUserResponse())),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent brukere feilet")));
    }
    
    [HttpPost("Users")]
    public async Task<ActionResult<UserAdminResponse>> CreateNewUsers([FromBody] UserUpsertRequest userToBeCreated)
    {
        var result = await _userService.CreateUser(userToBeCreated.MapToUserDto());
        return result.Match<ActionResult<UserAdminResponse>>(
            user => user.MapToUserResponse(),
            errors => BadRequest(errors.ToValidationProblemDetails("Opprettelse av bruker feilet")));
    }

    [HttpGet("Users/{userId:int}")]
    public async Task<ActionResult<UserAdminResponse>> GetUserById(int userId)
    {
        var result = await _userService.GetUserById(userId);
        return result is not null ? Ok(result.MapToUserResponse()) : NotFound();
    }

    [HttpPut("Users/{userId:int}")]
    public async Task<ActionResult<UserAdminResponse>> UpdateUsers([FromBody] UserUpsertRequest userToBeUpdated, int userId)
    {
        var updatedUser = await _userService.UpdateUser(userToBeUpdated.MapToUserDto(userId));
        return updatedUser.Match<ActionResult>(
            user => Ok(user.MapToUserResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Oppdatering av bruker feilet")));
    }

    [HttpPost("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<EmploymentRateResponse>> CreateEmploymentRateForUser(EmploymentRateUpsertRequest request, int userId)
    {
        var createdRate = await _userService.CreateEmploymentRateForUser(request.MapToEmploymentRateDto(userId, null));
        return createdRate.Match<ActionResult>(
            rate => Ok(rate.MapToEmploymentRateResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Opprettelse av ansettelsesrate feilet")));
    }

    [HttpPut("users/{userId:int}/employmentrates/{employmentRateId:int}")]
    public async Task<ActionResult<EmploymentRateResponse>> UpdateEmploymentRate(EmploymentRateUpsertRequest request, int userId, int employmentRateId)
    {
        var updatedRate = await _userService.UpdateEmploymentRateForUser(request.MapToEmploymentRateDto(userId, employmentRateId));
        return updatedRate.Match<ActionResult>(
            rate => Ok(rate.MapToEmploymentRateResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Oppdatering av ansettelsesrate feilet")));
    }
}