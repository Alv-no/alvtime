using System;
using System.Data;
using System.Linq;
using AlvTime.Business.Users;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using FluentValidation;
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
    public async Task CreateUser_UserEmployeeIdAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUsers(new[] { new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) } });
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.CreateUsers(new[] { new UserDto { Email = "user 2", Name = "user 2", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) } }));
    }

    [Fact]
    public async Task CreateUser_UserEmailAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUsers(new[] { new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) } });
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.CreateUsers(new[] { new UserDto { Email = "user 1", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01) } }));
    }

    [Fact]
    public async Task UpdateUser_UserEmployeeIdAlreadyExistsOnAnotherUser_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUsers(new[] { new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) } });
        var user2 = (await userService.CreateUsers(new[] { new UserDto { Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01) } })).First();
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.UpdateUsers(new[] { new UserDto { Id = user2.Id, EmployeeId = 1 } }));
    }

    [Fact]
    public async Task UpdateUser_UserEmailAlreadyExistsOnAnotherUser_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUsers(new[] { new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01) } });
        var user = (await userService.CreateUsers(new[] { new UserDto { Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01) } })).First();
        await Assert.ThrowsAsync<DuplicateNameException>(async () => await
            userService.UpdateUsers(new[] { new UserDto { Id = user.Id, Email = "user 1" } }));
    }

    [Fact]
    public async Task CreateEmploymentRate_EmploymentRateAlreadyExistsOnDate_RateIsNotCreated()
    {
        var userService = CreateUserService();
        await userService.CreateEmploymentRatesForUser(new[]
        {
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 01, 01),
                ToDateInclusive = new DateTime(2022, 05, 05),
                UserId = 1,
                Rate = 0.1M
            }
        });

        await Assert.ThrowsAsync<ValidationException>(async () => await userService.CreateEmploymentRatesForUser(new[]
        {
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            }
        }));
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

        await Assert.ThrowsAsync<ValidationException>(async () => await userService.CreateEmploymentRatesForUser(new[]
        {
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            }
        }));
    }

    [Fact]
    public async Task UpdateEmploymentRate_UserHasRegisteredHoursOnDate_RateIsNotUpdated()
    {
        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var employmentRate = await userService.CreateEmploymentRatesForUser(new[]
        {
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            }
        });

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

        await Assert.ThrowsAsync<ValidationException>(async () => await userService.UpdateEmploymentRatesForUser(new[]
        {
            new EmploymentRateChangeRequestDto
            {
                UserId = 1,
                RateId = employmentRate.First().Id,
                Rate = 0.3M,
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04)
            }
        }));
    }
    
    [Fact]
    public async Task UpdateEmploymentRate_UserHasRegisteredHoursOnDate_RateIsNotUpdated2()
    {
        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var employmentRate = await userService.CreateEmploymentRatesForUser(new[]
        {
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            }
        });

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

        await Assert.ThrowsAsync<ValidationException>(async () => await userService.UpdateEmploymentRatesForUser(new[]
        {
            new EmploymentRateChangeRequestDto
            {
                UserId = 1,
                RateId = employmentRate.First().Id,
                Rate = 0.3M,
                FromDateInclusive = new DateTime(2022, 04, 01),
                ToDateInclusive = new DateTime(2022, 04, 04)
            }
        }));
    }
    
    [Fact]
    public async Task UpdateEmploymentRate_UserHasNoRegisteredHoursOnDate_RateIsUpdated()
    {
        var userService = new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        var employmentRate = await userService.CreateEmploymentRatesForUser(new[]
        {
            new EmploymentRateDto
            {
                FromDateInclusive = new DateTime(2022, 03, 03),
                ToDateInclusive = new DateTime(2022, 04, 04),
                UserId = 1,
                Rate = 0.2M
            }
        });
        
        await userService.UpdateEmploymentRatesForUser(new[]
        {
            new EmploymentRateChangeRequestDto
            {
                UserId = 1,
                RateId = employmentRate.First().Id,
                Rate = 0.3M,
                FromDateInclusive = new DateTime(2022, 04, 01),
                ToDateInclusive = new DateTime(2022, 04, 04)
            }
        });

        var rate = await userService.GetCurrentEmploymentRateForUser(1, new DateTime(2022, 04, 02));
        
        Assert.Equal(0.3M, rate);
    }

    private UserService CreateUserService()
    {
        return new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));
    }
}