using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Requests;
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
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> CreateNewUsers([FromBody] IEnumerable<UserCreateRequest> usersToBeCreated)
    {
        var response = new List<UserResponseDto>();
        foreach (var user in usersToBeCreated)
        {
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
    [AuthorizeAdmin]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> UpdateUsers([FromBody] IEnumerable<UserUpdateRequest> usersToBeUpdated)
    {
        var response = new List<UserResponseDto>();
        foreach (var user in usersToBeUpdated)
        {
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
    [AuthorizeAdmin]
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
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> CreateEmploymentRateForUser(EmploymentRateCreationRequest request, int userId)
    {
        var createdEmploymentRate = await _userRepository.CreateEmploymentRateForUser(new EmploymentRateDto
        {
            UserId = userId,
            Rate = request.RatePercentage / 100,
            ToDateInclusive = request.ToDateInclusive.Date,
            FromDateInclusive = request.FromDateInclusive.Date
        });
        return Ok(new EmploymentRateResponse
        {
            Id = createdEmploymentRate.Id,
            UserId = createdEmploymentRate.UserId,
            RatePercentage = createdEmploymentRate.Rate * 100,
            FromDateInclusive = createdEmploymentRate.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = createdEmploymentRate.ToDateInclusive.ToDateOnly()
        });
    }

    [HttpPut("users/{userId:int}/employmentrates")]
    [AuthorizeAdmin]
    public async Task<ActionResult<EmploymentRateResponse>> UpdateEmploymentRate(EmploymentRateChangeRequest request, int userId)
    {
        var updatedEmploymentRate = await _userRepository.UpdateEmploymentRateForUser(new EmploymentRateChangeRequestDto
        {
            Rate = request.RatePercentage / 100,
            ToDateInclusive = request.ToDateInclusive,
            FromDateInclusive = request.FromDateInclusive,
            RateId = request.RateId
        });
        return Ok(new EmploymentRateResponse
        {
            Id = updatedEmploymentRate.Id,
            UserId = updatedEmploymentRate.UserId,
            RatePercentage = updatedEmploymentRate.Rate * 100,
            FromDateInclusive = updatedEmploymentRate.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = updatedEmploymentRate.ToDateInclusive.ToDateOnly()
        });
    }
}