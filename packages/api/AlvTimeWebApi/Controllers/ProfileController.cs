using AlvTime.Business.Options;
using AlvTime.Business.Users;
using AlvTime.Persistence.DataBaseModels;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ProfileController : Controller
    {
        private RetrieveUsers _userRetriever;
        private readonly IUserStorage _userStorage;
        private readonly int _reportUser;

        public ProfileController(RetrieveUsers userRetriever, IUserStorage userStorage, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            _userRetriever = userRetriever;
            _userStorage = userStorage;
            _reportUser = timeEntryOptions.CurrentValue.ReportUser;
        }

        [HttpGet("Profile")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<User> GetUserProfile()
        {
            return Ok(_userRetriever.RetrieveUser());
        }

        [HttpGet("UsersReport")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<UserResponseDto>> FetchUsersReport()
        {
          var user = _userRetriever.RetrieveUser();

          if (user.Id == _reportUser)
          {
            var users = _userStorage.GetUser(new UserQuerySearch());
            return Ok(users);
          }

          return Unauthorized();
        }
    }
}
