using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business;
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
            .WithUsers()
            .CreateDbContext();
    }

    [Fact]
    public void GetUsers_NoCriteria_AllUsers()
    {
        var userService = CreateUserService();

        var users = userService.GetUsers(new UserQuerySearch()).Result.Value;

        Assert.Equal(_context.User.Count(), users.Count);
    }
    
    [Fact]
    public void GetUsers_EmailIsGiven_AllUsersWithSpecifiedEmail()
    {
        var userService = CreateUserService();

        var user = userService.GetUsers(new UserQuerySearch
        {
            Email = "someone@alv.no",
        }).Result.Value;

        Assert.Equal("someone@alv.no", user.Single().Email);
    }
    
    [Fact]
    public void GetUsers_NameIsGiven_AllUsersWithSpecifiedName()
    {
        var userService = CreateUserService();

        var user = userService.GetUsers(new UserQuerySearch
        {
            Name = "Someone",
        }).Result.Value;

        Assert.Equal("Someone", user.Single().Name);
    }
    
    [Fact]
    public async Task CreateUser_NewUser_NewUserIsCreated()
    {
        var userService = CreateUserService();

        await userService.CreateUser(new UserDto
        {
            Email = "newUser@alv.no",
            Name = "New User",
            StartDate = DateTime.UtcNow,
            EmployeeId = 1
        });

        var createdUser = userService.GetUsers(new UserQuerySearch
        {
            Name = "New User"
        }).Result.Value;

        Assert.Single(createdUser);
    }
    
    [Fact]
    public async Task CreateUser_UserEmployeeIdAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) });
        var result = await
            userService.CreateUser(new UserDto
                { Email = "user 2", Name = "user 2", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) });
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Bruker med gitt ansattnummer finnes allerede.", result.Errors.First().Description);
    }

    [Fact]
    public async Task CreateUser_UserEmailAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) });
       var result = await
            userService.CreateUser(new UserDto
                { Email = "user 1", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01) });
       
       Assert.False(result.IsSuccess);
       Assert.Equal("Bruker med gitt epost finnes allerede.", result.Errors.First().Description);
    }
    
    [Fact]
    public async Task UserCreator_UpdateExistingUser_UserIsUpdated()
    {
        var userService = CreateUserService();

        await userService.UpdateUser(new UserDto
        {
            Id = 1,
            Email = "someoneElse@alv.no",
            Name = "SomeoneElse",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.Date
        });

        var user = await userService.GetUserById(1);

        Assert.Equal("someoneElse@alv.no", user.Email);
        Assert.Equal("SomeoneElse", user.Name);
        Assert.Equal(DateTime.UtcNow.Date, user.EndDate);
    }

    [Fact]
    public async Task UpdateUser_UserEmployeeIdAlreadyExistsOnAnotherUser_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) });
        var result = await userService.CreateUser(new UserDto
            { Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01) });
        var user2 = result.Match(user => user, _ => throw new Exception());
        var result2 = await
            userService.UpdateUser(new UserDto { Id = user2.Id, EmployeeId = 1 });
        
        Assert.False(result2.IsSuccess);
        Assert.Equal("En bruker har allerede blitt tildelt det ansattnummeret, eposten eller navnet.", result2.Errors.First().Description);
    }

    [Fact]
    public async Task UpdateUser_UserEmailAlreadyExistsOnAnotherUser_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUser(
            new UserDto
            {
                Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)
            });
        var result = (await userService.CreateUser(
            new UserDto
            {
                Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01)
            }));
        var user = result.Value;
        var result2 = await userService.UpdateUser(new UserDto { Id = user.Id, Email = "user 1" });
        
        Assert.False(result2.IsSuccess);
        Assert.Equal("En bruker har allerede blitt tildelt det ansattnummeret, eposten eller navnet.", result2.Errors.First().Description);
    }
    
    [Fact]
    public async Task CreateEmploymentRate_EmploymentRateOk_RateAdded()
    {
        var userService = CreateUserService();

        await userService.CreateEmploymentRateForUser(new EmploymentRateDto
        {
            UserId = 1,
            FromDateInclusive = new DateTime(2022, 01, 01),
            ToDateInclusive = new DateTime(2022, 01, 31),
            Rate = 0.5M
        });

        var rate = (await userService.GetCurrentEmploymentRateForUser(1, new DateTime(2022, 01, 15))).Value;
        Assert.Equal(0.5M, rate);
    }

    [Fact]
    public async Task CreateEmploymentRate_EmploymentRateAlreadyExistsOnDate_RateIsNotCreated()
    {
        var userService = CreateUserService();
        await userService.CreateEmploymentRateForUser(
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 01, 01),
                ToDateInclusive = new DateTime(2022, 05, 05),
                UserId = 1,
                Rate = 0.1M
            });

        var result = await userService.CreateEmploymentRateForUser(
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            });
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Brukeren har allerede en stillingsprosent på valgt dato.", result.Errors.First().Description);
    }

    [Fact]
    public async Task CreateEmploymentRate_UserHasRegisteredHoursOnDate_RateIsNotCreated()
    {
        _context.Hours.Add(new Hours
        {
            User = 1,
            Date = new DateTime(2022, 03, 28),
            DayNumber = 100,
            Id = 99,
            Locked = false,
            TaskId = 1,
            Value = 7.5m,
            Year = 2022
        });
        await _context.SaveChangesAsync();

        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var result = await userService.CreateEmploymentRateForUser(
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            });
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Endringen vil påvirke eksisterende timer.", result.Errors.First().Description);
    }

    [Fact]
    public async Task UpdateEmploymentRate_UserHasRegisteredHoursOnDate_RateIsNotUpdated()
    {
        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var result1 = await userService.CreateEmploymentRateForUser(
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            });
        var employmentRate = result1.Match(rate => rate, _ => throw new Exception());

        _context.Hours.Add(new Hours
        {
            User = 1,
            Date = new DateTime(2022, 03, 28),
            DayNumber = 100,
            Id = 99,
            Locked = false,
            TaskId = 1,
            Value = 7.5m,
            Year = 2022
        });
        await _context.SaveChangesAsync();

        var result2 = await userService.UpdateEmploymentRateForUser(
            new EmploymentRateDto
            {
                UserId = 1,
                RateId = employmentRate.Id,
                Rate = 0.3M,
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04)
            });
        
        Assert.False(result2.IsSuccess);
        Assert.Equal("Endringen vil påvirke eksisterende timer.", result2.Errors.First().Description);
    }

    [Fact]
    public async Task UpdateEmploymentRate_UserHasRegisteredHoursOnDate_RateIsNotUpdated2()
    {
        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var result = await userService.CreateEmploymentRateForUser(
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            });
        var employmentRate = result.Match(rate => rate, _ => throw new Exception());

        _context.Hours.Add(new Hours
        {
            User = 1,
            Date = new DateTime(2022, 03, 28),
            DayNumber = 100,
            Id = 99,
            Locked = false,
            TaskId = 1,
            Value = 7.5m,
            Year = 2022
        });
        await _context.SaveChangesAsync();

        var result2 = await userService.UpdateEmploymentRateForUser(
            new EmploymentRateDto
            {
                UserId = 1,
                RateId = employmentRate.Id,
                Rate = 0.3M,
                FromDateInclusive = new DateTime(2022, 04, 01),
                ToDateInclusive = new DateTime(2022, 04, 04)
            });
        
        Assert.False(result2.IsSuccess);
        Assert.Equal("Endringen vil påvirke eksisterende timer.", result2.Errors.First().Description);
    }

    [Fact]
    public async Task UpdateEmploymentRate_UserHasNoRegisteredHoursOnDate_RateIsUpdated()
    {
        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var result = await userService.CreateEmploymentRateForUser(
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            });
        var employmentRate = result.Match(rate => rate, _ => throw new Exception());

        await userService.UpdateEmploymentRateForUser(
            new EmploymentRateDto
            {
                UserId = 1,
                RateId = employmentRate.Id,
                Rate = 0.3M,
                FromDateInclusive = new DateTime(2022, 04, 01),
                ToDateInclusive = new DateTime(2022, 04, 04)
            });

        var rate = await userService.GetCurrentEmploymentRateForUser(1, new DateTime(2022, 04, 02));

        Assert.Equal(0.3M, rate.Value);
    }

    private UserService CreateUserService()
    {
        return new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));
    }
}