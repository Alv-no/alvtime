using AlvTime.Business.Customers;
using AlvTime.Business.HourRates;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories
{
    public class HourRateStorage : IHourRateStorage
    {
        private readonly AlvTime_dbContext _context;

        public HourRateStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public async Task CreateHourRate(CreateHourRateDto hourRate)
        {
            var newRate = new HourRate
            {
                FromDate = hourRate.FromDate,
                Rate = hourRate.Rate,
                TaskId = hourRate.TaskId
            };

            _context.HourRate.Add(newRate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHourRate(CreateHourRateDto hourRate)
        {
            var existingRate = await _context.HourRate
                .FirstOrDefaultAsync(hr => hr.FromDate == hourRate.FromDate && hr.TaskId == hourRate.TaskId);

            existingRate.Rate = hourRate.Rate;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<HourRateResponseDto>> GetHourRates(HourRateQuerySearch criterias)
        {
            return await _context.HourRate
                .Include(h => h.Task).ThenInclude(t => t.CompensationRate)
                .AsQueryable()
                .Filter(criterias)
                .Select(x => new HourRateResponseDto
                {
                    FromDate = x.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Id = x.Id,
                    Rate = x.Rate,
                    Task = new TaskResponseDto
                    {
                        Description = x.Task.Description,
                        Id = x.Task.Id,
                        Favorite = false,
                        Locked = x.Task.Locked,
                        Name = x.Task.Name,
                        CompensationRate = EnsureCompensationRate(x.Task.CompensationRate),
                        Project = new ProjectResponseDto
                        {
                            Id = x.Task.ProjectNavigation.Id,
                            Name = x.Task.ProjectNavigation.Name,
                            Customer = new CustomerDto
                            {
                                Id = x.Task.ProjectNavigation.CustomerNavigation.Id,
                                Name = x.Task.ProjectNavigation.CustomerNavigation.Name,
                                InvoiceAddress = x.Task.ProjectNavigation.CustomerNavigation.InvoiceAddress,
                                ContactPhone = x.Task.ProjectNavigation.CustomerNavigation.ContactPhone,
                                ContactEmail = x.Task.ProjectNavigation.CustomerNavigation.ContactEmail,
                                ContactPerson = x.Task.ProjectNavigation.CustomerNavigation.ContactPerson
                            }
                        },
                    }
                })
                .ToListAsync();
        }

        private static decimal EnsureCompensationRate(ICollection<CompensationRate> compensationRate)
        {
            return compensationRate.MaxBy(cr => cr.FromDate)?.Value ?? 0.0M;
        }
    }
}
