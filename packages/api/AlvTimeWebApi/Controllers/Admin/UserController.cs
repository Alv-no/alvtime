using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
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

        [HttpPost("Users")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> CreateNewUsers([FromBody] IEnumerable<CreateUserDto> usersToBeCreated)
        {
            List<UserResponseDto> response = new List<UserResponseDto>();
            foreach (var user in usersToBeCreated)
            {
                response.Add(_creator.CreateUser(new CreateUserDto
                {
                    Email = user.Email,
                    Name = user.Name,
                    StartDate = user.StartDate,
                    EndDate = user.EndDate
                }));
            }

            return Ok(response);
        }

        [HttpPut("Users")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> UpdateUsers([FromBody] IEnumerable<CreateUserDto> usersToBeUpdated)
        {
            List<UserResponseDto> response = new List<UserResponseDto>();
            foreach (var user in usersToBeUpdated)
            {
                response.Add(_creator.UpdateUser(user));
            }

            return Ok(response);
        }
    }
}
