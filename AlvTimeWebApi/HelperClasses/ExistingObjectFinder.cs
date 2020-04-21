using AlvTimeWebApi.Dto;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Linq;

namespace AlvTimeWebApi.HelperClasses
{
    public class ExistingObjectFinder
    {
        private readonly AlvTime_dbContext _database;

        public ExistingObjectFinder(AlvTime_dbContext database)
        {
            _database = database;
        }

        public bool TaskDoesNotExist(CreateTaskDto task)
        {
            return _database.Task
                    .Where(x => x.Name == task.Name && x.Project == task.Project)
                    .FirstOrDefault() == null ? true : false;
        }

        public bool CustomerDoesNotExist(CreateCustomerDto customer)
        {
            return _database.Customer
                    .Where(x => x.Name == customer.Name)
                    .FirstOrDefault() == null ? true : false;
        }

        public bool ProjectDoesNotExist(CreateProjectDto project)
        {
            return _database.Project
                    .Where(x => x.Name == project.Name && x.Customer == project.Customer)
                    .FirstOrDefault() == null ? true : false;
        }

        public bool HourRateDoesNotExist(CreateHourRateDto hourRate)
        {
            return _database.HourRate.FirstOrDefault(x =>
                x.FromDate.Date == hourRate.FromDate.Date &&
                x.Rate == hourRate.Rate &&
                x.TaskId == hourRate.TaskId) == null ? true : false;
        }

        public Hours RetrieveExistingTimeEntry(CreateTimeEntryDto hoursDto, User user)
        {
            return _database.Hours.FirstOrDefault(h =>
                h.Date.Date == hoursDto.Date.Date &&
                h.TaskId == hoursDto.TaskId &&
                h.User == user.Id);
        }

        public TaskFavorites RetrieveExistingFavorite(UpdateTasksDto taskDto, User user)
        {
            return _database.TaskFavorites.FirstOrDefault(x =>
                x.TaskId == taskDto.Id &&
                x.UserId == user.Id);
        }
    }
}
