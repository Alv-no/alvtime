using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.Tasks
{
    public class TaskService
    {
        private readonly ITaskStorage _taskStorage;
        private readonly IUserContext _userContext;

        public TaskService(ITaskStorage taskStorage, IUserContext userContext)
        {
            _taskStorage = taskStorage;
            _userContext = userContext;
        }

        public async Task<Result<IEnumerable<TaskResponseDto>>> GetTasksForUser(TaskQuerySearch criteria)
        {
            var currentUser = await _userContext.GetCurrentUser();
            var tasks = await _taskStorage.GetUsersTasks(criteria, currentUser.Id);

            return new Result<IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task<Result<IEnumerable<TaskResponseDto>>> UpdateFavoriteTasks(
            IEnumerable<(int id, bool favorite)> tasksToUpdate)
        {
            var currentUser = await _userContext.GetCurrentUser();

            List<TaskResponseDto> response = new List<TaskResponseDto>();
            foreach (var task in tasksToUpdate)
            {
                response.Add(await UpdateFavoriteTask(task.id, task.favorite, currentUser.Id));
            }

            return response;
        }

        private async Task<TaskResponseDto> UpdateFavoriteTask(int taskId, bool isTaskFavorited, int userId)
        {
            var userHasFavorite = await _taskStorage.IsFavorite(taskId, userId);

            if (userHasFavorite && !isTaskFavorited)
            {
                await _taskStorage.RemoveFavoriteTask(taskId, userId);
            }
            else if (!userHasFavorite && isTaskFavorited)
            {
                await _taskStorage.CreateFavoriteTask(taskId, userId);
            }

            return await GetTaskForUser(taskId, userId);
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
    }
}