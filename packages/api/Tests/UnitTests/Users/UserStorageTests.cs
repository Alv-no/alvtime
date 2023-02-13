using AlvTime.Business.Users;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.UnitTests.Users
{
    public class UserStorageTests
    {
        [Fact]
        public async Task GetUsers_NoCriterias_AllUsers()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserRepository(context);

            var users = storage.GetUsers(new UserQuerySearch()).Result.ToList();

            Assert.Equal(context.User.Count(), users.Count());
        }

        [Fact]
        public void GetUsers_EmailIsGiven_AllUsersWithSpecifiedEmail()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);
            var users = storage.GetUsers(new UserQuerySearch
            {
                Email = "someone@alv.no",
            }).Result.ToList();

            Assert.Equal("someone@alv.no", users.Single().Email);
        }

        [Fact]
        public void GetUsers_NameIsGiven_AllUsersWithSpecifiedName()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);
            var users = storage.GetUsers(new UserQuerySearch
            {
                Name = "Someone"
            }).Result.ToList();

            Assert.Equal("Someone", users.Single().Name);
        }

        [Fact]
        public async Task UserCreator_NewUser_NewUserIsCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var creator = new UserService(new UserRepository(context), new TimeRegistrationStorage(context));

            await creator.CreateUser(new UserDto
            {
                Email = "newUser@alv.no",
                Name = "New User",
                StartDate = DateTime.UtcNow,
                EmployeeId = 1
            });

            Assert.True(context.User.Count() == 3);
        }

        [Fact]
        public async Task UserCreator_UpdateExistingUser_UserIsUpdated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var creator = new UserService(new UserRepository(context), new TimeRegistrationStorage(context));

            await creator.UpdateUser(new UserDto
            {
                Id = 1,
                Email = "someoneElse@alv.no",
                Name = "SomeoneElse",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.Date
            });

            var user = context.User.FirstOrDefault(u => u.Id == 1);

            Assert.Equal("someoneElse@alv.no", user.Email);
            Assert.Equal("SomeoneElse", user.Name);
            Assert.Equal(DateTime.UtcNow.Date, user.EndDate);
        }
        
        [Fact]
        public async Task UserStorage_AddEmploymentRate_RateIsAdded()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);

            await storage.CreateEmploymentRateForUser(new EmploymentRateDto
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2022, 01, 01),
                ToDateInclusive = new DateTime(2022, 01, 31),
                Rate = 0.5M
            });

            var rate = await storage.GetEmploymentRates(new EmploymentRateQueryFilter
            {
                UserId = 1
            });
            Assert.Single(rate);
            Assert.Equal(0.5M, rate.First().Rate);
        }
        
        [Fact]
        public async Task UserStorage_UpdateEmploymentRate_RateIsUpdated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);

            var createdRate = await storage.CreateEmploymentRateForUser(new EmploymentRateDto
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2022, 01, 01),
                ToDateInclusive = new DateTime(2022, 01, 31),
                Rate = 0.5M
            });
            
            await storage.UpdateEmploymentRateForUser(new EmploymentRateChangeRequestDto
            {
                RateId = createdRate.Id,
                Rate = 0.6M
            });

            var rate = await storage.GetEmploymentRates(new EmploymentRateQueryFilter
            {
                UserId = 1
            });
            Assert.Single(rate);
            Assert.Equal(0.6M, rate.First().Rate);
        }
    }
}
