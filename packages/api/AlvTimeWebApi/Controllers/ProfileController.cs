using AlvTime.Business.Options;
using AlvTime.Business.Users;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class ProfileController(
    IUserContext userContext,
    IUserRepository userRepository,
    IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
    : Controller
{
    private readonly int _reportUser = timeEntryOptions.CurrentValue.ReportUser;

    [HttpGet("Profile")]
    public async Task<ActionResult<UserAdminResponse>> GetUserProfile()
    {
        var user = await userContext.GetCurrentUser();

        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new UserAdminResponse
        {
            Id = user.Id,
            StartDate = user.StartDate.ToDateOnly(),
            EndDate = user.EndDate?.ToDateOnly(),
            Email = user.Email,
            Name = user.Name,
            SalaryModel = user.SalaryModel,
        });
    }

    [HttpGet("UsersReport")]
    public async Task<ActionResult<IEnumerable<UserAdminResponse>>> FetchUsersReport()
    {
        var user = await userContext.GetCurrentUser();

        if (user.Id == _reportUser)
        {
            var users = await userRepository.GetUsers(new UserQuerySearch());
            return Ok(users.Select(u => u.MapToUserResponse()));
        }

        return Unauthorized();
    }
}