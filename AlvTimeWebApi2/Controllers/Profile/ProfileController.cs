using AlvTimeWebApi2.DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AlvTimeWebApi2.Controllers.Profile
{
    [Route("api/user")]
    [ApiController]
    public class ProfileController:Controller
    {
        private readonly AlvTime_dbContext _database;

        public ProfileController(AlvTime_dbContext database)
        {
            _database = database;
        }

        [HttpGet("Profile")]
        [Authorize]
        public ActionResult<User> GetUserProfile()
        {
            return Ok(RetrieveUser());
        }

        private User RetrieveUser()
        {
            var username = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var user = User.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var alvUser = _database.User.FirstOrDefault(x => x.Email.Equals(user));

            return alvUser;
        }
    }
}
