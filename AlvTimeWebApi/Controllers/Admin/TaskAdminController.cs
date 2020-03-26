using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Task = AlvTimeWebApi.DatabaseModels.Task;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class TaskAdminController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public TaskAdminController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpPost("CreateTask")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> CreateNewTask([FromBody] IEnumerable<CreateTaskDto> tasksToBeCreated)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeCreated)
            {
                if (checkExisting.TaskDoesNotExist(task))
                {
                    var newTask = new Task
                    {
                        Description = task.Description,
                        Favorite = false,
                        Locked = task.Locked,
                        Name = task.Name,
                        Project = task.Project
                    };
                    _database.Task.Add(newTask);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCreatedTask(task));
                }
            }
            return Ok(response);
        }

        [HttpPost("LockTask")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CreateTaskDto>> UpdateLockTask([FromBody] IEnumerable<UpdateTasksDto> tasksToBeUpdated)
        {
            var user = RetrieveUser();

            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                var taskToBeUpdated = _database.Task
                    .Where(x => x.Id == task.Id)
                    .FirstOrDefault();

                taskToBeUpdated.Locked = task.Locked;
                _database.SaveChanges();

                response.Add(returnObjects.ReturnTask(user, task));
            }
            return Ok(response);
        }

        private User RetrieveUser()
        {
            var username = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var email = User.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var alvUser = _database.User.FirstOrDefault(x => x.Email.Equals(email));

            return alvUser;
        }
    }
}