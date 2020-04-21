using AlvTime.Business;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin.Users
{
    [Route("api/admin")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;
        private readonly IUserStorage _userStorage;
        private readonly UserCreator _creator;

        public UserController(AlvTime_dbContext database, IUserStorage userStorage, UserCreator creator)
        {
            _database = database;
            _userStorage = userStorage;
            _creator = creator;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("Users")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> FetchUsers()
        {
            var users = _userStorage.GetUser(new UserQuerySearch());
            return Ok(users);
        }

        [HttpPost("CreateUser")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> CreateNewUser([FromBody] IEnumerable<CreateUserDto> usersToBeCreated)
        {
            List<UserResponseDto> response = new List<UserResponseDto>();
            foreach (var user in usersToBeCreated)
            {
                response.Add(_creator.CreateUser(new CreateUserRequest
                {
                    Email = user.Email,
                    FlexiHours = user.FlexiHours,
                    Name = user.Name,
                    StartDate = user.StartDate
                }));
            }

            return Ok(response);
        }
    }
}
