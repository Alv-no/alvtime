using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTime.Persistence.DataBaseModels;
using System.Collections.Generic;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public class TaskStorage : ITaskStorage
    {
        private readonly AlvTime_dbContext _context;

        public TaskStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public IEnumerable<TaskResponseDto> GetTasks(TaskQuerySearch criterias)
        {
            var tasks = _context.Task.AsQueryable()
                .Filter(criterias)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = false,
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

            var compensationRates = _context.CompensationRate.ToList().OrderByDescending(cr => cr.FromDate);

            foreach (var task in tasks)
            {
                task.CompensationRate = compensationRates.Any(cr => cr.TaskId == task.Id) ?
                    compensationRates.First(cr => cr.TaskId == task.Id).Value :
                    0.0M;
            }

            return tasks;
        }

        public IEnumerable<TaskResponseDto> GetUsersTasks(TaskQuerySearch criterias, int userId)
        {
            var usersFavoriteTaskIds = _context.TaskFavorites.Where(x => x.UserId == userId).Select(x => x.TaskId).ToList();

            var tasks = GetTasks(criterias);

            foreach (var task in tasks)
            {
                task.Favorite = usersFavoriteTaskIds.Contains(task.Id);
            }

            return tasks;
        }

        public void CreateTask(CreateTaskDto task)
        {
            var newTask = new Task
            {
                Description = task.Description,
                Favorite = false,
                Locked = task.Locked,
                Name = task.Name,
                Project = task.Project,
                FillPriority = 1
            };
            _context.Task.Add(newTask);
            _context.SaveChanges();
        }

        public void UpdateTask(UpdateTasksDto taskToBeUpdated)
        {
            var existingTask = _context.Task
                   .Where(x => x.Id == taskToBeUpdated.Id)
                   .FirstOrDefault();

            if (taskToBeUpdated.FillPriority != null)
            {
                existingTask.FillPriority = (int)taskToBeUpdated.FillPriority;
            }
            if (taskToBeUpdated.Locked != null)
            {
                existingTask.Locked = (bool)taskToBeUpdated.Locked;
            }
            if (taskToBeUpdated.Name != null)
            {
                existingTask.Name = taskToBeUpdated.Name;
            }
            if (taskToBeUpdated.CompensationRate != null)
            {
                var compensationRates = _context.CompensationRate.ToList().OrderByDescending(cr => cr.FromDate);
                var compRateToBeUpdated = compensationRates.First(cr => cr.TaskId == taskToBeUpdated.Id);
                compRateToBeUpdated.Value = (decimal)taskToBeUpdated.CompensationRate;
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
