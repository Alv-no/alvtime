using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Tasks.Admin;

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

        public IEnumerable<TaskResponseDto> GetTasksForUser(TaskQuerySearch criteria)
        {
            var currentUser = _userContext.GetCurrentUser();
            var tasks = _taskStorage.GetUsersTasks(criteria, currentUser.Id);

            return tasks;
        }
        
        public IEnumerable<TaskResponseDto> GetAllTasks(TaskQuerySearch criteria)
        {
            return _taskStorage.GetTasks(criteria);
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
        
        public IEnumerable<TaskResponseDto> CreateTasks(IEnumerable<CreateTaskDto> tasksToBeCreated)
        {
            var response = new List<TaskResponseDto>();
            
            foreach (var task in tasksToBeCreated)
            {
                if (!GetTask(task).Any())
                {
                    _taskStorage.CreateTask(task);
                }
                response.Add(GetTask(task).Single());
            }

            return response;
        }

        public IEnumerable<TaskResponseDto> UpdateTasks(IEnumerable<UpdateTaskDto> tasksToBeUpdated)
        {
            var response = new List<TaskResponseDto>();
            foreach (var task in tasksToBeUpdated)
            {
                _taskStorage.UpdateTask(task);
                response.Add(_taskStorage.GetTasks(new TaskQuerySearch
                {
                    Id = task.Id
                }).Single());
            }

            return response;
        }

        private IEnumerable<TaskResponseDto> GetTask(CreateTaskDto task)
        {
            return _taskStorage.GetTasks(new TaskQuerySearch
            {
                Name = task.Name,
                Project = task.Project
            });
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