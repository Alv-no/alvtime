﻿using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;

    public UserController(IUserRepository userRepository, UserService userService)
    {
        _userRepository = userRepository;
        _userService = userService;
    }

    [HttpGet("Users")]
    [AuthorizeAdmin]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> FetchUsers()
    {
        var users = await _userRepository.GetUsers(new UserQuerySearch());
        return Ok(users);
    }

    [HttpPost("Users")]
    [AuthorizeAdmin]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> CreateNewUsers([FromBody] IEnumerable<UserDto> usersToBeCreated)
    {
        List<UserResponseDto> response = new List<UserResponseDto>();
        foreach (var user in usersToBeCreated)
        {
            response.Add(await _userService.CreateUser(new UserDto
            {
                Email = user.Email,
                Name = user.Name,
                StartDate = user.StartDate,
                EndDate = user.EndDate
            }));
        }

        return Ok(response);
    }

    [HttpPut("Users")]
    [AuthorizeAdmin]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> UpdateUsers([FromBody] IEnumerable<UserDto> usersToBeUpdated)
    {
        List<UserResponseDto> response = new List<UserResponseDto>();
        foreach (var user in usersToBeUpdated)
        {
            response.Add(await _userService.UpdateUser(user));
        }

        return Ok(response);
    }

    [HttpGet("user/{userId}/employmentrates")]
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> FetchEmploymentRatesForUser(int userId)
    {
        return Ok((await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId })).Select(er => new EmploymentRateResponse
        {
            Id = er.Id,
            UserId = er.UserId,
            Rate = er.Rate,
            FromDateInclusive = er.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = er.ToDateInclusive.ToDateOnly()
        }));
    }

    [HttpPost("user/employmentrate")]
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> CreateEmploymentRateForUser(EmploymentRateDto request)
    {
        var createdEmploymentRate = await _userRepository.CreateEmploymentRateForUser(request);
        return Ok(new EmploymentRateResponse
        {
            Id = createdEmploymentRate.Id,
            UserId = createdEmploymentRate.UserId,
            Rate = createdEmploymentRate.Rate,
            FromDateInclusive = createdEmploymentRate.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = createdEmploymentRate.ToDateInclusive.ToDateOnly()
        });
    }

    [HttpPut("User/employmentrate")]
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> UpdateEmploymentRate(EmploymentRateChangeRequest request)
    {
        var updatedEmploymentRate = await _userRepository.UpdateEmploymentRateForUser(request);
        return Ok(new EmploymentRateResponse
        {
            Id = updatedEmploymentRate.Id,
            UserId = updatedEmploymentRate.UserId,
            Rate = updatedEmploymentRate.Rate,
            FromDateInclusive = updatedEmploymentRate.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = updatedEmploymentRate.ToDateInclusive.ToDateOnly()
        });
    }
}