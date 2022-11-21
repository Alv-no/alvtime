using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using AlvTime.Business.TimeRegistration;
using System;
using AlvTimeWebApi.Controllers.Utils;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
public class AnalyticsController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;
    private readonly TimeRegistrationService _timeRegistrationService;

    public AnalyticsController(IUserRepository userRepository, UserService userService, TimeRegistrationService timeRegistrationService)
    {
        _userRepository = userRepository;
        _userService = userService;
        _timeRegistrationService = timeRegistrationService;
    }

    [HttpGet("Data")]
    [AuthorizeAdmin]
    public async Task<List<string>> FetchData()
    {
        var users = await _userRepository.GetUsers(new UserQuerySearch());
        var allResults = new List<string>();
        foreach(var user in users) {
            var userModel = UserMapper.MapUserDtoToBusinessUser(user);
            var availableOvertime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(DateTime.Now, userModel);
            allResults.AddRange(availableOvertime.UnCompensatedOvertime.Select(entry =>
                $"{user.Email},{entry.Date.ToDateOnly()},{entry.Hours},{entry.CompensationRate},OvertimeHours"
                ).ToList());
            allResults.AddRange(availableOvertime.CompensatedFlexHours.Select(entry =>
                $"{user.Email},{entry.Date.ToDateOnly()},{entry.Hours},{entry.CompensationRate},FlexHours"
                ).ToList());
            allResults.AddRange(availableOvertime.CompensatedPayouts.Select(entry =>
                $"{user.Email},{entry.Date.ToDateOnly()},{entry.Hours},{entry.CompensationRate},PayoutHours"
                ).ToList());
        }
        return allResults;
    }
}