using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;

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

        public IEnumerable<TaskResponseDto> GetTasks()
        {
            var currentUser = _userContext.GetCurrentUser();
            var tasks = _taskStorage.GetUsersTasks(new TaskQuerySearch(), currentUser.Id);

            return tasks;
        }

        public IEnumerable<TaskResponseDto> UpdateFavoriteTasks(IEnumerable<(int id, bool favorite)> tasksToUpdate)
        {
            var currentUser = _userContext.GetCurrentUser();
            
            List<TaskResponseDto> response = new List<TaskResponseDto>();
            foreach (var task in tasksToUpdate)
            {
                response.Add(UpdateFavoriteTasks(task.id, task.favorite, currentUser.Id));
            }

            return response;
        }
        
        private TaskResponseDto UpdateFavoriteTasks(int taskId, bool isTaskFavorited, int userId)
        {
            var userHasFavorite = _taskStorage.GetFavorite(taskId, userId);

            if (userHasFavorite && !isTaskFavorited)
            {
                _taskStorage.RemoveFavoriteTask(taskId, userId);
            }
            else if (!userHasFavorite && isTaskFavorited)
            {
                _taskStorage.CreateFavoriteTask(taskId, userId);
            }

            return GetTask(taskId, userId);
        }
        
        private TaskResponseDto GetTask(int taskId, int userId)
        {
            var taskResponseDto = _taskStorage.GetUsersTasks(new TaskQuerySearch
            {
                Id = taskId
            }, userId);

            return taskResponseDto.Single();
        }
    }
}