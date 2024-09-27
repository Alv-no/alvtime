using AlvTime.Business.Customers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.CompensationRate;
using AlvTime.Business.HourRates;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories;

public class CustomerStorage : ICustomerStorage
{
    private readonly AlvTime_dbContext _context;

    public CustomerStorage(AlvTime_dbContext context)
    {
        _context = context;
    }

    public async Task CreateCustomer(CustomerDto customer)
    {
        var newCustomer = new Customer
        {
            Name = customer.Name,
            InvoiceAddress = customer.InvoiceAddress ?? "",
            ContactPhone = customer.ContactPhone ?? "",
            ContactEmail = customer.ContactEmail ?? "",
            ContactPerson = customer.ContactPerson ?? "",
            OrgNr = customer.OrgNr
        };

        _context.Customer.Add(newCustomer);
        await _context.SaveChangesAsync();
    }

    public async Task<CustomerAdminDto?> GetCustomerDetailedById(int customerId)
    {
        return await _context.Customer
            .Where(c => c.Id == customerId)
            .Include(c => c.Project)
            .ThenInclude(p => p.Task)
            .ThenInclude(t => t.HourRate)
            .Include(c => c.Project)
            .ThenInclude(p => p.Task)
            .ThenInclude(t => t.CompensationRate)
            .Select(customer => new CustomerAdminDto
            {
                Id = customer.Id,
                Name = customer.Name,
                InvoiceAddress = customer.InvoiceAddress,
                ContactPerson = customer.ContactPerson,
                ContactEmail = customer.ContactEmail,
                ContactPhone = customer.ContactPhone,
                OrgNr = customer.OrgNr,
                Projects = customer.Project.Select(p => new ProjectAdminDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    TaskCount = p.Task.Count(), 
                    Tasks = p.Task.Select(t => new TaskAdminDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        CompensationRate = EnsureCompensationRate(t.CompensationRate),
                        Locked = t.Locked,
                        ProjectId = p.Id, 
                        ProjectName = p.Name,
                        HourRates = t.HourRate.Select(hr => new HourRateDto
                        {
                            Id = hr.Id,
                            Rate = hr.Rate,
                            FromDate = hr.FromDate,
                            TaskId = t.Id, 
                            TaskName = t.Name
                        }).ToList()
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomers(CustomerQuerySearch criterias)
    {
        return await _context.Customer.AsQueryable()
            .Filter(criterias)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                ContactPerson = c.ContactPerson,
                ContactEmail = c.ContactEmail,
                ContactPhone = c.ContactPhone,
                InvoiceAddress = c.InvoiceAddress,
                OrgNr = c.OrgNr
            }).ToListAsync();
    }

    public async Task<IEnumerable<CustomerAdminDto>> GetCustomersDetailed()
    {
        return await _context.Customer
            .Include(c => c.Project)
            .ThenInclude(p => p.Task)
            .ThenInclude(t => t.HourRate)
            .Include(c => c.Project)
            .ThenInclude(p => p.Task)
            .ThenInclude(t => t.CompensationRate)
            .Select(customer => new CustomerAdminDto
            {
                Id = customer.Id,
                Name = customer.Name,
                InvoiceAddress = customer.InvoiceAddress,
                ContactPerson = customer.ContactPerson,
                ContactEmail = customer.ContactEmail,
                ContactPhone = customer.ContactPhone,
                OrgNr = customer.OrgNr,
                Projects = customer.Project.Select(p => new ProjectAdminDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Tasks = p.Task.Select(t => new TaskAdminDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        CompensationRate = EnsureCompensationRate(t.CompensationRate),
                        Locked = t.Locked,
                        HourRates = t.HourRate.Select(hr => new HourRateDto
                        {
                            Id = hr.Id,
                            Rate = hr.Rate,
                            FromDate = hr.FromDate
                        })
                    })
                })
            }).ToListAsync();
    }

    private static decimal EnsureCompensationRate(IEnumerable<CompensationRate> compensationRate)
    {
        return compensationRate.MaxBy(cr => cr.FromDate)?.Value ?? 0.0M;
    }

    public async Task UpdateCustomer(CustomerDto customer)
    {
        var existingCustomer = await _context.Customer
            .FirstOrDefaultAsync(x => x.Id == customer.Id);

        existingCustomer.Name = customer.Name;
        existingCustomer.ContactEmail = customer.ContactEmail;
        existingCustomer.ContactPerson = customer.ContactPerson;
        existingCustomer.ContactPhone = customer.ContactPhone;
        existingCustomer.InvoiceAddress = customer.InvoiceAddress;
        existingCustomer.OrgNr = customer.OrgNr;

        await _context.SaveChangesAsync();
    }
}