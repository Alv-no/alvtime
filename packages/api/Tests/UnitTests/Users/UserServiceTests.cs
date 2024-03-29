﻿using System;
using System.Data;
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
    
    [Fact]
    public async Task CreateEmploymentRate_EmploymentRateAlreadyExistsOnDate_RateIsNotCreated()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)});
        await userService.CreateEmploymentRateForUser(new EmploymentRateDto
        {
            FromDateInclusive = new DateTime(2022, 01, 01),
            ToDateInclusive = new DateTime(2022, 05, 05),
            UserId = 1,
            Rate = 0.1M
        });
        
        await Assert.ThrowsAsync<ValidationException>(async () => await userService.CreateEmploymentRateForUser(new EmploymentRateDto
        {
            FromDateInclusive = new DateTime(2022, 03, 03),
            ToDateInclusive = new DateTime(2022, 04, 04),
            UserId = 1,
            Rate = 0.2M
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
        
        var userService =  new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));

        await userService.CreateUser(new UserDto { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01)});
 
        await Assert.ThrowsAsync<ValidationException>(async () => await userService.CreateEmploymentRateForUser(new EmploymentRateDto
        {
            FromDateInclusive = new DateTime(2022, 03, 03),
            ToDateInclusive = new DateTime(2022, 04, 04),
            UserId = 1,
            Rate = 0.2M
        }));
    }

    public UserService CreateUserService()
    {
        return new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));
    }
}