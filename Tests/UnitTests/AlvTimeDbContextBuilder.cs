using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace Tests.UnitTests
{
    public class AlvTimeDbContextBuilder
    {
        private AlvTime_dbContext _context;

        public AlvTimeDbContextBuilder()
        {
            var options = new DbContextOptionsBuilder<AlvTime_dbContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

            _context = new AlvTime_dbContext(options);
        }

        public AlvTime_dbContext CreateDbContext()
        {
            return _context;
        }

        public AlvTimeDbContextBuilder WithPersonalAccessTokens()
        {
            _context.AccessTokens.Add(new AccessTokens
            {
                Id = 1,
                UserId = 1,
                Value = "123",
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithHourRates()
        {
            _context.HourRate.Add(new HourRate
            {
                Id = 1,
                FromDate = new DateTime(2019, 01, 01),
                Rate = 1000,
                TaskId = 1
            });

            _context.HourRate.Add(new HourRate
            {
                Id = 2,
                FromDate = new DateTime(2020, 01, 01),
                Rate = 1100,
                TaskId = 1
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithTaskFavorites()
        {
            _context.TaskFavorites.Add(new TaskFavorites
            {
                Id = 1,
                UserId = 1,
                TaskId = 1
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithCustomers()
        {
            _context.Customer.Add(new Customer
            {
                Id = 1,
                Name = "ExampleCustomer"
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithTimeEntries()
        {
            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2019, 05, 01),
                DayNumber = 145,
                Id = 1,
                Locked = false,
                TaskId = 1,
                Value = 8,
                Year = 2019
            });

            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2019, 05, 02),
                DayNumber = 145,
                Id = 2,
                Locked = false,
                TaskId = 1,
                Value = 6,
                Year = 2019
            });

            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2019, 05, 02),
                DayNumber = 145,
                Id = 3,
                Locked = false,
                TaskId = 2,
                Value = 6,
                Year = 2019
            });

            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 05, 02),
                DayNumber = 145,
                Id = 4,
                Locked = false,
                TaskId = 1,
                Value = 6,
                Year = 2019
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithProjects()
        {
            _context.Project.Add(new Project
            {
                Id = 1,
                Name = "ExampleProject",
                Customer = 1
            });

            _context.Project.Add(new Project
            {
                Id = 2,
                Name = "ExampleProjectTwo",
                Customer = 1
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithTasks()
        {
            _context.Task.Add(new Task
            {
                Id = 1,
                Description = "",
                Project = 1,
                CompensationRate = 1.0M,
                Name = "ExampleTask",
                Locked = false
            });

            _context.Task.Add(new Task
            {
                Id = 2,
                Description = "",
                Project = 2,
                CompensationRate = 1.5M,
                Name = "ExampleTaskTwo",
                Locked = true
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithUsers()
        {
            _context.User.Add(new User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                FlexiHours = 150,
                StartDate = DateTime.Now
            });

            _context.User.Add(new User
            {
                Id = 2,
                Email = "someone2@alv.no",
                Name = "Someone2",
                FlexiHours = 10,
                StartDate = DateTime.Now
            });

            _context.SaveChanges();
            return this;
        }
    }
}