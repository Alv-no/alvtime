using System;
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

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
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
        var users = await _userService.GetUsers(new UserQuerySearch());
        return Ok(users.Select(u => u.MapToUserResponse()));
    }
    
    [HttpPost("Users")]
    public async Task<ActionResult<IEnumerable<UserAdminResponse>>> CreateNewUsers([FromBody] List<UserCreateRequest> usersToBeCreated)
    {
        var createdUsers = await _userService.CreateUsers(usersToBeCreated.Select(u => u.MapToUserDto()));
        return Ok(createdUsers.Select(u => u.MapToUserResponse()));
    }

    [HttpPut("Users")]
    public async Task<ActionResult<IEnumerable<UserAdminResponse>>> UpdateUsers([FromBody] List<UserUpdateRequest> usersToBeUpdated)
    {
        var updatedUsers = await _userService.UpdateUsers(usersToBeUpdated.Select(u => u.MapToUserDto()));
        return Ok(updatedUsers.Select(u => u.MapToUserResponse()));
    }
    
    [Obsolete("Will be deleted")]
    [HttpGet("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> FetchEmploymentRatesForUser(int userId)
    {
        var rates = (await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId })).Select(er => er.MapToEmploymentRateResponse()).ToList();
        return Ok(rates.OrderByDescending(rate => rate.FromDateInclusive));
    }

    [HttpPost("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> CreateEmploymentRateForUser(List<EmploymentRateCreateRequest> requests, int userId)
    {
        var createdRates = await _userService.CreateEmploymentRatesForUser(requests.Select(r => r.MapToEmploymentRateDto(userId)));
        return Ok(createdRates.Select(rate => rate.MapToEmploymentRateResponse()));
    }

    [HttpPut("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> UpdateEmploymentRate(List<EmploymentRateUpdateRequest> requests, int userId)
    {
        var updatedRates = await _userService.UpdateEmploymentRatesForUser(requests.Select(r => r.MapToEmploymentRateChangeRequestDto(userId)));
        return Ok(updatedRates.Select(rate => rate.MapToEmploymentRateResponse()));
    }
}