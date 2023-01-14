using System;
using System.Data;
using AlvTime.Business.Users;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Users;

public class UserServiceTests
{
    private readonly AlvTime_dbContext _context;

    public UserServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .CreateDbContext();
    }
    
    [Fact]
    public async Task CreateUser_UserEmployeeIdAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        var user1 = await userService.CreateUser(new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)});
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.CreateUser(new UserDto { Email = "user 2", Name = "user 2", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) }));
    }
    
    [Fact]
    public async Task CreateUser_UserEmailAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        var user1 = await userService.CreateUser(new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)});
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.CreateUser(new UserDto { Email = "user 1", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01) }));
    }
    
    [Fact]
    public async Task UpdateUser_UserEmployeeIdAlreadyExistsOnAnotherUser_ExceptionThrown()
    {
        var userService = CreateUserService();
        var user1 = await userService.CreateUser(new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)});
        var user2 = await userService.CreateUser(new UserDto { Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01)});
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.UpdateUser(new UserDto { Id = user2.Id, EmployeeId = 1 }));
    }
    
    [Fact]
    public async Task UpdateUser_UserEmailAlreadyExistsOnAnotherUser_ExceptionThrown()
    {
        var userService = CreateUserService();
        var user1 = await userService.CreateUser(new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)});
        var user2 = await userService.CreateUser(new UserDto { Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01)});
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.UpdateUser(new UserDto { Id = user2.Id, Email = "user 1" }));
    }

    public UserService CreateUserService()
    {
        return new UserService(new UserRepository(_context));
    }
}