using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace Tests.UnitTests
{
    public class AlvTimeDbContextBuilder
    {
        public AlvTime_dbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AlvTime_dbContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

            var context = new AlvTime_dbContext(options);

            context.Task.Add(new Task
            {
                Id = 1,
                Description = "",
                Project = 1,
                CompensationRate = 1.0M,
                Name = "ExampleTask",
                Locked = false
            });

            context.Task.Add(new Task
            {
                Id = 2,
                Description = "",
                Project = 2,
                CompensationRate = 1.5M,
                Name = "ExampleTaskTwo",
                Locked = true
            });

            context.Project.Add(new Project
            {
                Id = 1,
                Name = "ExampleProject",
                Customer = 1
            });

            context.Project.Add(new Project
            {
                Id = 2,
                Name = "ExampleProjectTwo",
                Customer = 1
            });

            context.Customer.Add(new Customer
            {
                Id = 1,
                Name = "ExampleCustomer"
            });

            context.TaskFavorites.Add(new TaskFavorites
            {
                Id = 1,
                UserId = 1,
                TaskId = 1
            });

            context.User.Add(new User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                FlexiHours = 150,
                StartDate = DateTime.Now
            });

            context.User.Add(new User
            {
                Id = 2,
                Email = "someone2@alv.no",
                Name = "Someone2",
                FlexiHours = 10,
                StartDate = DateTime.Now
            });

            context.Hours.Add(new Hours
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

            context.Hours.Add(new Hours
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

            context.Hours.Add(new Hours
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

            context.Hours.Add(new Hours
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

            context.HourRate.Add(new HourRate
            {
                Id = 1,
                FromDate = new DateTime(2019, 01, 01),
                Rate = 1000,
                TaskId = 1
            });

            context.HourRate.Add(new HourRate
            {
                Id = 2,
                FromDate = new DateTime(2020, 01, 01),
                Rate = 1100,
                TaskId = 1
            });

            context.SaveChanges();

            return context;
        }
    }
}