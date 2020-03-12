using AlvTimeApi.Dto;
using AlvTimeWebApi2.DataBaseModels;
using AlvTimeWebApi2.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeApi.Controllers.Tasks
{
    [Route("api/admin")]
    [ApiController]
    public class EconomyController : Controller
    {
        private readonly AlvTimeDBContext _database;

        public EconomyController(AlvTimeDBContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Retrieves tasks
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpGet("Something")]
        [Authorize]
        public ActionResult<IEnumerable<TaskResponseDto>> FetchSomething()
        {
            var user = RetrieveUser();

            var returnValue = new List<TaskResponseDto>();

            return returnValue;
        }

        private User RetrieveUser()
        {

            //var username = HttpContext.User.Identity.Name ?? "NameNotFound";
            //var user = _database.User.FirstOrDefault(x => x.Email.Trim() == username.Trim());
            var user = _database.User.FirstOrDefault();

            return user;
        }
    }
}
