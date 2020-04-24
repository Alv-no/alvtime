using AlvTime.Business;
using AlvTime.Business.Users;
using AlvTimeWebApi.Controllers.Admin.Users.UserStorage;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Users
{
    public class UserStorageTests
    {
        [Fact]
        public void GetUsers_NoCriterias_AllUsers()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserStorage(context);

            var users = storage.GetUser(new UserQuerySearch()).ToList();

            Assert.Equal(context.User.Count(), users.Count());
        }

        [Fact]
        public void GetUsers_EmailIsGiven_AllUsersWithSpecifiedEmail()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserStorage(context);
            var users = storage.GetUser(new UserQuerySearch
            {
                Email = "someone@alv.no",
            }).ToList();

            Assert.Equal("someone@alv.no", users.Single().Email);
        }

        [Fact]
        public void GetUsers_NameIsGiven_AllUsersWithSpecifiedName()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserStorage(context);
            var users = storage.GetUser(new UserQuerySearch
            {
                Name = "Someone"
            }).ToList();

            Assert.Equal("Someone", users.Single().Name);
        }

        [Fact]
        public void UserCreator_NewUser_NewUserIsCreated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserStorage(context);
            var creator = new UserCreator(storage, new AlvHoursCalculator());

            creator.CreateUser(new CreateUserRequest
            {
                Email = "newUser@alv.no",
                FlexiHours = 10,
                Name = "New User",
                StartDate = DateTime.UtcNow
            });

            Assert.True(context.User.Count() == 3);
        }

        [Fact]
        public void UserCreator_UserAlreadyExists_NoUserIsCreated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserStorage(context);
            var creator = new UserCreator(storage, new AlvHoursCalculator());

            creator.CreateUser(new CreateUserRequest
            {
                Email = "someone@alv.no",
                FlexiHours = 150,
                Name = "Someone",
                StartDate = DateTime.UtcNow
            });

            Assert.True(context.User.Count() == 2);
        }
    }
}
