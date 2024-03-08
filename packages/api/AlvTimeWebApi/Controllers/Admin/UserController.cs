using System;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using FluentValidation;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
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
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> FetchUsers()
    {
        var users = await _userRepository.GetUsers(new UserQuerySearch());
        return Ok(users);
    }
    
    [HttpPost("Users")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> CreateNewUsers([FromBody] IEnumerable<UserCreateRequest> usersToBeCreated)
    {
        var response = new List<UserResponseDto>();
        foreach (var user in usersToBeCreated)
        {
            if (user.EndDate.HasValue && user.StartDate >= user.EndDate)
            {
                return BadRequest($"Sluttdato må være etter startdato for {user.Name}");
            }
            response.Add(await _userService.CreateUser(new UserDto
            {
                Name = user.Name,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                StartDate = user.StartDate,
                EndDate = user.EndDate
            }));
        }

        return Ok(response);
    }

    [HttpPut("Users")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> UpdateUsers([FromBody] IEnumerable<UserUpdateRequest> usersToBeUpdated)
    {
        var response = new List<UserResponseDto>();
        foreach (var user in usersToBeUpdated)
        {
            if (user.StartDate.HasValue && user.EndDate.HasValue && user.StartDate.Value >= user.EndDate.Value)
            {
                return BadRequest($"Sluttdato må være etter startdato for {user.Name}");
            }

            response.Add(await _userService.UpdateUser(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                StartDate = user.StartDate ,
                EndDate = user.EndDate
            }));
        }

        return Ok(response);
    }

    [HttpGet("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> FetchEmploymentRatesForUser(int userId)
    {
        return Ok((await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId })).Select(er => new EmploymentRateResponse
        {
            Id = er.Id,
            UserId = er.UserId,
            RatePercentage = er.Rate * 100,
            FromDateInclusive = er.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = er.ToDateInclusive.ToDateOnly()
        }).OrderByDescending(er => er.ToDateInclusive));
    }

    [HttpPost("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> CreateEmploymentRateForUser(IEnumerable<EmploymentRateCreationRequest> requests, int userId)
    {
        var response = new List<EmploymentRateResponse>();
        foreach (var req in requests)
        {
            var createdEmploymentRate = await _userService.CreateEmploymentRateForUser(new EmploymentRateDto
            {
                UserId = userId,
                Rate = req.RatePercentage / 100M,
                ToDateInclusive = req.ToDateInclusive.Date,
                FromDateInclusive = req.FromDateInclusive.Date
            });
            response.Add(new EmploymentRateResponse
            {
                Id = createdEmploymentRate.Id,
                UserId = createdEmploymentRate.UserId,
                RatePercentage = createdEmploymentRate.Rate * 100M,
                FromDateInclusive = createdEmploymentRate.FromDateInclusive.ToDateOnly(),
                ToDateInclusive = createdEmploymentRate.ToDateInclusive.ToDateOnly()
            });
        }

        return Ok(response);
    }

    [HttpPut("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> UpdateEmploymentRate(IEnumerable<EmploymentRateChangeRequest> requests, int userId)
    {
        var response = new List<EmploymentRateResponse>();
        foreach (var req in requests)
        {
            var updatedEmploymentRate = await _userService.UpdateEmploymentRateForUser(new EmploymentRateChangeRequestDto
            {
                Rate = req.RatePercentage / 100,
                ToDateInclusive = req.ToDateInclusive,
                FromDateInclusive = req.FromDateInclusive,
                RateId = req.Id,
                UserId = userId
            });
            response.Add(new EmploymentRateResponse
            {
                Id = updatedEmploymentRate.Id,
                UserId = updatedEmploymentRate.UserId,
                RatePercentage = updatedEmploymentRate.Rate * 100,
                FromDateInclusive = updatedEmploymentRate.FromDateInclusive.ToDateOnly(),
                ToDateInclusive = updatedEmploymentRate.ToDateInclusive.ToDateOnly()
            });
        }

        return Ok(response);
    }
}