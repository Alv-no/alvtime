using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Task = AlvTimeWebApi.Persistence.DatabaseModels.Task;

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
                        Project = task.Project,
                        CompensationRate = task.CompensationRate
                    };
                    _database.Task.Add(newTask);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCreatedTask(task));
                }
            }
            return Ok(response);
        }

        [HttpPost("UpdateTask")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> UpdateTask([FromBody] IEnumerable<UpdateTasksDto> tasksToBeUpdated)
        {
            var user = RetrieveUser();

            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                var existingTask = _database.Task
                    .Where(x => x.Id == task.Id)
                    .FirstOrDefault();

                if(task.Locked != null)
                {
                    existingTask.Locked = (bool)task.Locked;
                }
                if(task.CompensationRate != null)
                {
                    existingTask.CompensationRate = (decimal)task.CompensationRate;
                }
                if(task.Name != null)
                {
                    existingTask.Name = task.Name;
                }

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