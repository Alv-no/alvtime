using AlvTime.Business.Options;
using AlvTime.Business.Users;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;

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
        public ActionResult<UserAdminResponse> GetUserProfile()
        {
            var user = _userRetriever.RetrieveUser();

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
                Name = user.Name
            });
        }

        [HttpGet("UsersReport")]
        public async Task<ActionResult<IEnumerable<UserAdminResponse>>> FetchUsersReport()
        {
            var user = _userRetriever.RetrieveUser();

            if (user.Id == _reportUser)
            {
                var users = await _userRepository.GetUsers(new UserQuerySearch());
                return Ok(users.Select(u => u.MapToUserResponse()));
            }

            return Unauthorized();
        }
    }
}
