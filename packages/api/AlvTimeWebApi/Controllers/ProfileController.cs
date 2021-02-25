using AlvTime.Persistence.DataBaseModels;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ProfileController : Controller
    {
        private RetrieveUsers _userRetriever;

        public ProfileController(RetrieveUsers userRetriever)
        {
            _userRetriever = userRetriever;
        }

        [HttpGet("Profile")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<User> GetUserProfile()
        {
            return Ok(_userRetriever.RetrieveUser());
        }
    }
}
