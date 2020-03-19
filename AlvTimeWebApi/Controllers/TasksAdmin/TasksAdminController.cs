using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.TasksAdmin
{
    [Route("api/admin")]
    [ApiController]
    public class TasksAdminController : Controller
    {
        private readonly AlvTime_dbContext _database;

        public TasksAdminController(AlvTime_dbContext database)
        {
            _database = database;
        }

        [HttpPost("CreateTask")]
        [Authorize]
        public ActionResult<IEnumerable<TaskResponseDto>> CreateNewTask([FromBody] IEnumerable<CreateTaskDto> tasksToBeCreated)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeCreated)
            {
                var newTask = new Task
                {
                    Description = task.Description,
                    Favorite = false,
                    Locked = task.Locked,
                    Name = task.Name,
                    Project = task.Project
                };
                _database.Add(newTask);
                _database.SaveChanges();

                response.Add(ReturnCreatedTask(task));
            }
            return Ok(response);
        }

        [HttpPost("DeleteTask")]
        [Authorize]
        public ActionResult<IEnumerable<CreateTaskDto>> DeleteExistingTask([FromBody] IEnumerable<DeleteTaskDto> tasksToBeDeleted)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeDeleted)
            {
                response.Add(ReturnDeletedTask(task));

                var taskToBeDeleted = _database.Task
                    .Where(x => x.Id == task.Id)
                    .Single();

                _database.Remove(taskToBeDeleted);
                _database.SaveChanges();
            }
            return Ok(response);
        }

        private TaskResponseDto ReturnCreatedTask(CreateTaskDto task)
        {
            var taskResponseDto = _database.Task
                .Where(x => x.Name == task.Name && x.Project == task.Project)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = false,
                    Project = new ProjectDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    },
                })
                .Single();

            return taskResponseDto;
        }

        private TaskResponseDto ReturnDeletedTask(DeleteTaskDto task)
        {
            var taskResponseDto = _database.Task
                .Where(x => x.Id == task.Id)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = false,
                    Project = new ProjectDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    },
                })
                .Single();

            return taskResponseDto;
        }
    }
}
