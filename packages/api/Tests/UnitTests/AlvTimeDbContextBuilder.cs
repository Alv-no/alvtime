using AlvTime.Persistence.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Tests.UnitTests
{
    public class AlvTimeDbContextBuilder
    {
        private AlvTime_dbContext _context;
        private const int AbsenceProject = 9;

        public AlvTimeDbContextBuilder(bool isSqlite = false)
        {
            if (isSqlite)
            {
                var options = new DbContextOptionsBuilder<AlvTime_dbContext>()
                    .UseSqlite(CreateInMemoryDatabase())
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options; 
                
                _context = new AlvTime_dbContext(options);
                _context.Database.EnsureCreated();
            }
            else
            {
                var options = new DbContextOptionsBuilder<AlvTime_dbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), db => db.EnableNullChecks(false))
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;
                
                _context = new AlvTime_dbContext(options);
            }
        }

        private DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
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
            
            _context.Project.Add(new Project
            {
                Id = AbsenceProject,
                Name = "AbsenceProject",
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
                Name = "ExampleTask"
            });

            _context.Task.Add(new Task
            {
                Id = 2,
                Description = "",
                Project = 2,
                Name = "ExampleTaskTwo"
            });
            
            _context.Task.Add(new Task
            {
                Id = 3,
                Description = "",
                Project = 2,
                Name = "ExampleTaskThree"
            });
            
            _context.Task.Add(new Task
            {
                Id = 4,
                Description = "",
                Project = 2,
                Name = "ExampleTaskFour",
                Locked = true
            });

            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 1,
                Value = 1.5M,
                FromDate = new DateTime(2019, 01 ,01)
            });

            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 2,
                Value = 1.0M,
                FromDate = new DateTime(2019, 01 ,01)
            });
            
            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 3,
                Value = 0.5M,
                FromDate = new DateTime(2019, 01 ,01)
            });

            _context.SaveChanges();
            return this;
        }

        public AlvTimeDbContextBuilder WithLeaveTasks()
        {
            _context.Task.Add(new Task
            {
                Id = 12,
                Description = "",
                Project = AbsenceProject,
                Name = "UnpaidHoliday",
                Locked = false,
            });

            _context.Task.Add(new Task
            {
                Id = 13,
                Description = "",
                Project = AbsenceProject,
                Name = "PaidHoliday",
                Locked = false
            });

            _context.Task.Add(new Task
            {
                Id = 14,
                Description = "",
                Project = AbsenceProject,
                Name = "SickDay",
                Locked = false
            });
            
            _context.Task.Add(new Task
            {
                Id = 18,
                Description = "",
                Project = AbsenceProject,
                Name = "Flex",
                Locked = false
            });
            
            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 12,
                Value = 1.0M,
                FromDate = new DateTime(2019, 01 ,01)
            });

            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 13,
                Value = 1.0M,
                FromDate = new DateTime(2019, 01 ,01)
            });
            
            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 14,
                Value = 1.0M,
                FromDate = new DateTime(2019, 01 ,01)
            });
            
            _context.CompensationRate.Add(new CompensationRate
            {
                TaskId = 18,
                Value = 1.0M,
                FromDate = new DateTime(2019, 01 ,01)
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
                StartDate = new DateTime(2020, 01, 02)
            });

            _context.User.Add(new User
            {
                Id = 2,
                Email = "someone2@alv.no",
                Name = "Someone2",
                StartDate = DateTime.Now
            });

            _context.SaveChanges();
            return this;
        }
    }
}