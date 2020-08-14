using AlvTime.Business.Customers;
using AlvTime.Business.HourRates;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using AlvTime.Persistence.DataBaseModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public class HourRateStorage : IHourRateStorage
    {
        private readonly AlvTime_dbContext _context;

        public HourRateStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public void CreateHourRate(CreateHourRateDto hourRate)
        {
            var newRate = new HourRate
            {
                FromDate = hourRate.FromDate,
                Rate = hourRate.Rate,
                TaskId = hourRate.TaskId
            };

            _context.HourRate.Add(newRate);
            _context.SaveChanges();
        }

        public void UpdateHourRate(CreateHourRateDto hourRate)
        {
            var existingRate = _context.HourRate
                .Where(hr => hr.FromDate == hourRate.FromDate && hr.TaskId == hourRate.TaskId)
                .FirstOrDefault();

            existingRate.Rate = hourRate.Rate;
            _context.SaveChanges();
        }

        public IEnumerable<HourRateResponseDto> GetHourRates(HourRateQuerySearch criterias)
        {
            var hourRates = _context.HourRate.AsQueryable()
                .Filter(criterias)
                .Select(x => new HourRateResponseDto
                {
                    FromDate = x.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Id = x.Id,
                    Rate = x.Rate,
                    Task = _context.Task
                    .Where(y => y.Id == x.TaskId)
                    .Select(y => new TaskResponseDto
                    {
                        Description = y.Description,
                        Id = y.Id,
                        Favorite = false,
                        Locked = y.Locked,
                        Name = y.Name,
                        CompensationRate = y.CompensationRate,
                        Project = new ProjectResponseDto
                        {
                            Id = y.ProjectNavigation.Id,
                            Name = y.ProjectNavigation.Name,
                            Customer = new CustomerDto
                            {
                                Id = y.ProjectNavigation.CustomerNavigation.Id,
                                Name = y.ProjectNavigation.CustomerNavigation.Name,
                                InvoiceAddress = y.ProjectNavigation.CustomerNavigation.InvoiceAddress,
                                ContactPhone = y.ProjectNavigation.CustomerNavigation.ContactPhone,
                                ContactEmail = y.ProjectNavigation.CustomerNavigation.ContactEmail,
                                ContactPerson = y.ProjectNavigation.CustomerNavigation.ContactPerson
                            }
                        }
                    })
                    .FirstOrDefault()
                })
                .ToList();

            return hourRates;
        }
    }
}
