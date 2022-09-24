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
        public void GetUsers_NoCriterias_AllUsers()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new UserRepository(context);

            var users = storage.GetUsers(new UserQuerySearch()).ToList();

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
            }).ToList();

            Assert.Equal("someone@alv.no", users.Single().Email);
        }

        [Fact]
        public void GetUsers_NameIsGiven_AllUsersWithSpecifiedName()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);
            var users = await storage.GetUsers(new UserQuerySearch
            {
                Name = "Someone"
            }).ToList();

            Assert.Equal("Someone", users.Single().Name);
        }

        [Fact]
        public async Task UserCreator_NewUser_NewUserIsCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);
            var creator = new UserService(storage);

            await creator.CreateUser(new UserDto
            {
                Email = "newUser@alv.no",
                Name = "New User",
                StartDate = DateTime.UtcNow
            });

            Assert.True(context.User.Count() == 3);
        }

        [Fact]
        public async Task UserCreator_UserAlreadyExists_NoUserIsCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);
            var creator = new UserService(storage);

            await creator.CreateUser(new UserDto
            {
                Email = "someone@alv.no",
                Name = "Someone",
                StartDate = DateTime.UtcNow
            });

            Assert.True(context.User.Count() == 2);
        }

        [Fact]
        public void UserCreator_UpdateExistingUser_UserIsUpdated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new UserRepository(context);
            var creator = new UserService(storage);

            creator.UpdateUser(new UserDto
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
    }
}
