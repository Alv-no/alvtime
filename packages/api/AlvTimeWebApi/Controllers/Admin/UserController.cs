using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("Users")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> FetchUsers()
        {
            var users = _userService.GetUsers(new UserQuerySearch());
            return Ok(users);
        }

        [HttpPost("Users")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> CreateNewUsers([FromBody] IEnumerable<CreateUserDto> usersToBeCreated)
        {
            List<UserResponseDto> response = new List<UserResponseDto>();
            foreach (var user in usersToBeCreated)
            {
                response.Add(_userService.CreateUser(new CreateUserDto
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
                response.Add(_userService.UpdateUser(user));
            }

            return Ok(response);
        }
    }
}
