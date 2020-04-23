using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Tasks.TaskStorage
{
    public class TaskStorage : ITaskStorage
    {
        private readonly AlvTime_dbContext _context;

        public TaskStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public IEnumerable<TaskResponseDto> GetTasks(TaskQuerySearch criterias, int userId)
        {
            var favoriteList = _context.TaskFavorites.Where(x => x.UserId == userId).Select(x => x.TaskId).ToList();

            var tasks = _context.Task.AsQueryable()
                .Filter(criterias)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    CompensationRate = x.CompensationRate,
                    Project = new ProjectResponseDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name,
                            ContactEmail = x.ProjectNavigation.CustomerNavigation.ContactEmail,
                            ContactPerson = x.ProjectNavigation.CustomerNavigation.ContactPerson,
                            ContactPhone = x.ProjectNavigation.CustomerNavigation.ContactPhone,
                            InvoiceAddress = x.ProjectNavigation.CustomerNavigation.InvoiceAddress
                        }
                    }
                }).ToList();

            tasks.ForEach(x => x.Favorite = favoriteList.Contains(x.Id) ? true : false);

            return tasks;
        }

        public void CreateTask(CreateTaskDto task, int userId)
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
            _context.Task.Add(newTask);
            _context.SaveChanges();
        }

        public void UpdateTask(UpdateTasksDto taskToBeUpdated, int userId)
        {
            var existingTask = _context.Task
                   .Where(x => x.Id == taskToBeUpdated.Id)
                   .FirstOrDefault();

            if (taskToBeUpdated.Locked != null)
            {
                existingTask.Locked = (bool)taskToBeUpdated.Locked;
            }
            if (taskToBeUpdated.CompensationRate != null)
            {
                existingTask.CompensationRate = (decimal)taskToBeUpdated.CompensationRate;
            }
            if (taskToBeUpdated.Name != null)
            {
                existingTask.Name = taskToBeUpdated.Name;
            }

            _context.SaveChanges();
        }

        public void CreateFavoriteTask(int taskId, int userId)
        {
            TaskFavorites favorite = new TaskFavorites
            {
                TaskId = taskId,
                UserId = userId
            };
            _context.TaskFavorites.Add(favorite);
            _context.SaveChanges();
        }

        public void RemoveFavoriteTask(int taskId, int userId)
        {
            var favoriteEntry = _context.TaskFavorites
                .FirstOrDefault(tf => tf.UserId == userId && tf.TaskId == taskId);

            _context.TaskFavorites.Remove(favoriteEntry);
            _context.SaveChanges();
        }

        public bool GetFavorite(UpdateTasksDto taskToBeUpdated, int userId)
        {
            return _context.TaskFavorites
                .FirstOrDefault(tf => tf.UserId == userId && tf.TaskId == taskToBeUpdated.Id) == null ? false : true;
        }
    }
}
