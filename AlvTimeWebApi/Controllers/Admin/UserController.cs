using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public UserController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("Users")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> FetchUsers()
        {
            var users = _database.User
                .Select(x => new UserResponseDto
                {
                    Id = x.Id,
                    Email = x.Email,
                    Name = x.Name,
                    FlexiHours = x.FlexiHours,
                    StartDate = x.StartDate
                }).ToList();

            return Ok(users);
        }

        [HttpPost("CreateUser")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<UserResponseDto>> CreateNewUser([FromBody] IEnumerable<CreateUserDto> usersToBeCreated)
        {
            List<UserResponseDto> response = new List<UserResponseDto>();

            foreach (var user in usersToBeCreated)
            {
                if (checkExisting.UserDoesNotExist(user))
                {
                    var newUser = new User
                    {
                        Name = user.Name,
                        Email = user.Email,
                        StartDate = user.StartDate,
                        FlexiHours = user.FlexiHours
                    };
                    _database.User.Add(newUser);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCreatedUser(user));
                }
            }
            return Ok(response);
        }
    }
}
