using System;
using AlvTime.Business.Options;
using AlvTime.Business.Users;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using AlvTimeWebApi.Authorization;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    [AuthorizePersonalAccessToken]
    public class ProfileController : Controller
    {
        private RetrieveUsers _userRetriever;
        private readonly IUserRepository _userRepository;
        private readonly int _reportUser;

        public ProfileController(RetrieveUsers userRetriever, IUserRepository userRepository, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            _userRetriever = userRetriever;
            _userRepository = userRepository;
            _reportUser = timeEntryOptions.CurrentValue.ReportUser;
        }

        [HttpGet("Profile")]
        public ActionResult<UserResponseDto> GetUserProfile()
        {
            var user = _userRetriever.RetrieveUser();

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(new UserResponseDto()
            {
                Id = user.Id,
                StartDate = user.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EndDate = user.EndDate != null ? ((DateTime)user.EndDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : null,
                Email = user.Email,
                Name = user.Name
            });
        }

        [HttpGet("UsersReport")]
        public ActionResult<IEnumerable<UserResponseDto>> FetchUsersReport()
        {
            var user = _userRetriever.RetrieveUser();

            if (user.Id == _reportUser)
            {
                var users = _userRepository.GetUsers(new UserQuerySearch());
                return Ok(users);
            }

            return Unauthorized();
        }
    }
}
