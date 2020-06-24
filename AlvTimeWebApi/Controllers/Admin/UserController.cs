using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin.Users
{
    [Route("api/admin")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserStorage _userStorage;
        private readonly UserCreator _creator;

        public UserController(IUserStorage userStorage, UserCreator creator)
        {
            _userStorage = userStorage;
            _creator = creator;
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
                response.Add(_creator.CreateUser(new CreateUserDto
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
