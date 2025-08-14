using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;

namespace AlvTime.Business.Tasks
{
    public class TaskService
    {
        private readonly TimeRegistrationService _timeRegistrationService;
        private readonly ITaskStorage _taskStorage;
        private readonly IUserContext _userContext;

        public TaskService(TimeRegistrationService timeRegistrationService, ITaskStorage taskStorage, IUserContext userContext)
        {
            _timeRegistrationService = timeRegistrationService;
            _taskStorage = taskStorage;
            _userContext = userContext;
        }

        public async Task<Result<IEnumerable<TaskResponseDto>>> GetTasksForUser(TaskQuerySearch criteria)
        {
            var currentUser = await _userContext.GetCurrentUser();
            var tasks = await _taskStorage.GetUsersTasks(criteria, currentUser.Id);

            return new Result<IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task UpdateFavoriteTasks(
            IEnumerable<UpdateTaskDto> tasksToUpdate)
        {
            var currentUser = await _userContext.GetCurrentUser();

            foreach (var task in tasksToUpdate)
            {
                await UpdateFavoriteTask(task, currentUser.Id);
            }
        }

        private async Task UpdateFavoriteTask(UpdateTaskDto task, int userId)
        {
            var userHasFavorite = await _taskStorage.IsFavorite(task.Id, userId);

            if (userHasFavorite && !task.Favorite)
            {
                await _taskStorage.RemoveFavoriteTask(task.Id, userId);
            }
            else if (!userHasFavorite && task.Favorite)
            {
                await _taskStorage.CreateFavoriteTask(task.Id, userId);
            }
            await _taskStorage.ToggleCommentsOnFavoriteTask(task.Id, task.EnableComments, userId);
        }

        public async Task<Result<TaskDto>> CreateTask(TaskDto taskToBeCreated, int projectId)
        {
            var taskAlreadyExists = (await GetTask(taskToBeCreated.Name, projectId)).Any();
            if (taskAlreadyExists)
            {
                return new Error(ErrorCodes.EntityAlreadyExists, "En timekode med det navnet finnes allerede på prosjektet");
            }

            await _taskStorage.CreateTask(taskToBeCreated, projectId);
            return (await GetTask(taskToBeCreated.Name, projectId)).First();
        }

        public async Task<Result<TaskDto>> UpdateTask(TaskDto taskToBeUpdated)
        {
            await _taskStorage.UpdateTask(taskToBeUpdated);
            var task = await GetTaskById(taskToBeUpdated.Id);

            return new TaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Locked = task.Locked,
                CompensationRate = task.CompensationRate,
                Imposed = task.Imposed
            };
        }

        private async Task<IEnumerable<TaskDto>> GetTask(string taskName, int projectId)
        {
            var task = await _taskStorage.GetTasks(new TaskQuerySearch
            {
                Name = taskName,
                Project = projectId
            });

            return task.Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Locked = t.Locked,
                CompensationRate = t.CompensationRate,
                Imposed = t.Imposed
            });
        }

        private async Task<TaskDto> GetTaskById(int taskId)
        {
            var task = (await _taskStorage.GetTasks(new TaskQuerySearch
            {
                Id = taskId
            })).First();

            return new TaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Locked = task.Locked,
                CompensationRate = task.CompensationRate,
                Imposed = task.Imposed
            };
        }

        private async Task<TaskResponseDto> GetTaskForUser(int taskId, int userId)
        {
            var taskResponseDto = await _taskStorage.GetUsersTasks(new TaskQuerySearch
            {
                Id = taskId
            }, userId);

            return taskResponseDto.Single();
        }
        
        public async Task<IEnumerable<TaskResponseDto>> GetLatestTasksForUser()
        {
            var timeEntries = await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = DateTime.Now.AddDays(-30),
                ToDateInclusive = DateTime.Now
            });

            var mostRecentlyUsedTaskIds = timeEntries.OrderByDescending(item => item.Date).Select(dto => dto.TaskId)
                .Distinct().Take(5).ToList();
            var taskResponses = new List<TaskResponseDto>();
        
            foreach (var taskId in mostRecentlyUsedTaskIds)
            {
                var tasksForUser = await GetTasksForUser(new TaskQuerySearch { Id = taskId });
                if (tasksForUser.IsSuccess)
                    taskResponses.AddRange(tasksForUser.Value);
            }

            return taskResponses;
        }
    }
}