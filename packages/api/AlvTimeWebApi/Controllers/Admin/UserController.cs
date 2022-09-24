using System;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public ActionResult<IEnumerable<UserResponseDto>> FetchUsers()
    {
        var users = _userRepository.GetUsers(new UserQuerySearch());
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
        return Ok(await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId }));
    }

    [HttpPost("user/employmentrate")]
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> CreateEmploymentRateForUser(EmploymentRateDto request)
    {
        return await _userRepository.CreateEmploymentRateForUser(request); 
    }

    [HttpPut("User/employmentrate")]
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> UpdateEmploymentRate(EmploymentRateChangeRequest request)
    {
        return await _userRepository.UpdateEmploymentRateForUser(request);
    }
}