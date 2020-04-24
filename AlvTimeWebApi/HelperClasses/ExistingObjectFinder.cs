using AlvTime.Business.HourRates;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTime.Business.TimeEntries;
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
    }
}
