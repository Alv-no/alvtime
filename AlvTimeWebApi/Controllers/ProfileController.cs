using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Profile
{
    [Route("api/user")]
    [ApiController]
    public class ProfileController:Controller
    {
        private RetrieveUsers _userRetriever;

        public ProfileController(RetrieveUsers userRetriever)
        {
            _userRetriever = userRetriever;
        }

        [HttpGet("Profile")]
        [Authorize]
        public ActionResult<User> GetUserProfile()
        {
            return Ok(_userRetriever.RetrieveUser());
        }
    }
}
