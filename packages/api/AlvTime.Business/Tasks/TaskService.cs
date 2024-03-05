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

        public async Task<IEnumerable<TaskResponseDto>> GetTasksForUser(TaskQuerySearch criteria)
        {
            var currentUser = await _userContext.GetCurrentUser();
            var tasks = await _taskStorage.GetUsersTasks(criteria, currentUser.Id);

            return tasks;
        }
        
        public async Task<IEnumerable<TaskResponseDto>> GetAllTasks(TaskQuerySearch criteria)
        {
            return await _taskStorage.GetTasks(criteria);
        }

        public async Task<IEnumerable<TaskResponseDto>> UpdateFavoriteTasks(IEnumerable<(int id, bool favorite)> tasksToUpdate)
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
        
        public async Task<IEnumerable<TaskResponseDto>> CreateTasks(IEnumerable<CreateTaskDto> tasksToBeCreated)
        {
            var response = new List<TaskResponseDto>();
            
            foreach (var task in tasksToBeCreated)
            {
                var taskAlreadyExists = (await GetTask(task)).Any();
                if (!taskAlreadyExists)
                {
                    await _taskStorage.CreateTask(task);
                }
                response.Add((await GetTask(task)).Single());
            }

            return response;
        }

        public async Task<IEnumerable<TaskResponseDto>> UpdateTasks(IEnumerable<UpdateTaskDto> tasksToBeUpdated)
        {
            var response = new List<TaskResponseDto>();
            foreach (var task in tasksToBeUpdated)
            {
                await _taskStorage.UpdateTask(task);
                var responseDto = (await _taskStorage.GetTasks(new TaskQuerySearch
                {
                    Id = task.Id
                })).Single();
                response.Add(responseDto);
            }

            return response;
        }

        private async Task<IEnumerable<TaskResponseDto>> GetTask(CreateTaskDto task)
        {
            return await _taskStorage.GetTasks(new TaskQuerySearch
            {
                Name = task.Name,
                Project = task.Project
            });
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