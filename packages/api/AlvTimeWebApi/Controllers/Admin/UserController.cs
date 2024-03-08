using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class UserController : Controller
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("Users")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> FetchUsers()
    {
        var users = await _userService.GetUsers(new UserQuerySearch());
        return Ok(users.Select(u => u.MapToUserResponse()));
    }
    
    [HttpPost("Users")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> CreateNewUsers([FromBody] List<UserCreateRequest> usersToBeCreated)
    {
        var createdUsers = await _userService.CreateUsers(usersToBeCreated.Select(u => u.MapToUserDto()));
        return Ok(createdUsers.Select(u => u.MapToUserResponse()));
    }

    [HttpPut("Users")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> UpdateUsers([FromBody] List<UserUpdateRequest> usersToBeUpdated)
    {
        var updatedUsers = await _userService.UpdateUsers(usersToBeUpdated.Select(u => u.MapToUserDto()));
        return Ok(updatedUsers.Select(u => u.MapToUserResponse()));
    }

    [HttpPost("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> CreateEmploymentRateForUser(List<EmploymentRateCreationRequest> requests, int userId)
    {
        var createdRates = await _userService.CreateEmploymentRatesForUser(requests.Select(r => r.MapToEmploymentRateDto(userId)));
        return Ok(createdRates.Select(rate => rate.MapToEmploymentRateResponse()));
    }

    [HttpPut("users/{userId:int}/employmentrates")]
    public async Task<ActionResult<IEnumerable<EmploymentRateResponse>>> UpdateEmploymentRate(List<EmploymentRateChangeRequest> requests, int userId)
    {
        var updatedRates = await _userService.UpdateEmploymentRatesForUser(requests.Select(r => r.MapToEmploymentRateChangeRequestDto(userId)));
        return Ok(updatedRates.Select(rate => rate.MapToEmploymentRateResponse()));
    }
}