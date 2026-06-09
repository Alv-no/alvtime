using System;
using System.Linq;
using AlvTime.Business.Overtime;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Tests.UnitTests.TestUtils;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Users;

public class UserServiceTests
{
    private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithStaticSalaryUsers()
        .CreateDbContext();

    [Fact]
    public async Task GetUsers_NoCriteria_AllUsers()
    {
        var userService = CreateUserService();

        var users = (await userService.GetUsers(new UserQuerySearch())).Value;

        Assert.Equal(_context.User.Count(), users.Count);
    }
    
    [Fact]
    public async Task GetUsers_EmailIsGiven_AllUsersWithSpecifiedEmail()
    {
        var userService = CreateUserService();

        var user = (await userService.GetUsers(new UserQuerySearch
        {
            Email = "someone@alv.no",
        })).Value;

        Assert.Equal("someone@alv.no", user.Single().Email);
    }
    
    [Fact]
    public async Task GetUsers_NameIsGiven_AllUsersWithSpecifiedName()
    {
        var userService = CreateUserService();

        var user = (await userService.GetUsers(new UserQuerySearch
        {
            Name = "Someone",
        })).Value;

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
            EmployeeId = 1,
            Oid = "12345678-1234-1234-1234-123456789012"
        });

        var createdUser = (await userService.GetUsers(new UserQuerySearch
        {
            Name = "New User"
        })).Value;

        Assert.Single(createdUser);
    }
    
    [Fact]
    public async Task CreateUser_UserEmployeeIdAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01), Oid = "12345678-1234-1234-1234-123456789012" });
        var result = await
            userService.CreateUser(new UserDto
                { Email = "user 2", Name = "user 2", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01), Oid = "23456789-2345-2345-2345-234567890123" });
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Bruker med gitt ansattnummer finnes allerede.", result.Errors.First().Description);
    }

    [Fact]
    public async Task CreateUser_ActiveUserEmailAlreadyExists_ExceptionThrown()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01), Oid = "12345678-1234-1234-1234-123456789012" });
       var result = await
            userService.CreateUser(new UserDto
                { Email = "user 1", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01), Oid = "23456789-2345-2345-2345-234567890123" });
       
       Assert.False(result.IsSuccess);
       Assert.Equal("Aktiv bruker med gitt epost finnes allerede.", result.Errors.First().Description);
    }
    
    [Fact]
    public async Task CreateUser_InactiveUserEmailAlreadyExists_UserCreated()
    {
        var userService = CreateUserService();
        await userService.CreateUser(new UserDto
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01), EndDate = new DateTime(2020, 01, 01), Oid = "12345678-1234-1234-1234-123456789012" });
        var result = await
            userService.CreateUser(new UserDto
                { Email = "user 1", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01), Oid = "23456789-2345-2345-2345-234567890123" });
       
        Assert.True(result.IsSuccess);
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
            EndDate = DateTime.UtcNow.Date,
            Oid = "12345678-1234-1234-1234-123456789012"
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
            { Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01), Oid = "12345678-1234-1234-1234-123456789012" });
        var result = await userService.CreateUser(new UserDto
            { Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01), Oid = "23456789-2345-2345-2345-234567890123" });
        var user2 = result.Match(user => user, _ => throw new Exception());
        var result2 = await
            userService.UpdateUser(new UserDto { Id = user2.Id, EmployeeId = 1, Oid = "23456789-2345-2345-2345-234567890123" });
        
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
                Email = "user 1", Name = "user 1", EmployeeId = 1, StartDate = new DateTime(1900, 01, 01), Oid = "12345678-1234-1234-1234-123456789012"
            });
        var result = (await userService.CreateUser(
            new UserDto
            {
                Email = "user 2", Name = "user 2", EmployeeId = 2, StartDate = new DateTime(1900, 01, 01), Oid = "23456789-2345-2345-2345-234567890123"
            }));
        var user = result.Value;
        var result2 = await userService.UpdateUser(new UserDto { Id = user.Id, Email = "user 1", Oid = "23456789-2345-2345-2345-234567890123" });
        
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

    [Fact]
    public async Task UpdateSalaryModel_TodayBeforeJune1_SchedulesForThisYearJune1()
    {
        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        Assert.True(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(new DateTime(2026, 6, 1), history[1].SwitchDate);
        Assert.Equal(SalaryModel.Static, history[1].PreviousModel);
        Assert.Equal(SalaryModel.InvoiceBased, history[1].NewModel);
    }

    [Fact]
    public async Task UpdateSalaryModel_TodayOnJune1_AppliesRetroactivelyToThisYearJune1()
    {
        var clock = CreateClock(new DateTime(2026, 6, 1));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        Assert.True(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(new DateTime(2026, 6, 1), history[1].SwitchDate);
    }

    [Fact]
    public async Task UpdateSalaryModel_TodayInJuneAndUserHasntChangedThisYear_AppliesRetroactivelyToThisYearJune1()
    {
        var clock = CreateClock(new DateTime(2026, 6, 15));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        Assert.True(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(new DateTime(2026, 6, 1), history[1].SwitchDate);
        Assert.Equal(SalaryModel.InvoiceBased, history[1].NewModel);
    }

    [Fact]
    public async Task UpdateSalaryModel_TodayInJulyOrLater_ReturnsError()
    {
        var clock = CreateClock(new DateTime(2026, 7, 1));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateSalaryModel_TodayInJuneButUserAlreadyChangedThisYear_ReturnsError()
    {
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = 1,
            SwitchDate = new DateTime(2026, 6, 1),
            PreviousModel = SalaryModel.Static,
            NewModel = SalaryModel.InvoiceBased
        });
        await _context.SaveChangesAsync();

        var clock = CreateClock(new DateTime(2026, 6, 15));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.Static);

        Assert.False(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(SalaryModel.InvoiceBased, history[1].NewModel);
    }

    [Fact]
    public async Task UpdateSalaryModel_ResubmittingSamePending_NoDuplicateRow()
    {
        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);

        await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);
        await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(SalaryModel.InvoiceBased, history[1].NewModel);
    }

    [Fact]
    public async Task UpdateSalaryModel_UserCurrentlyOnInvoiceBased_SchedulesSwitchToStatic()
    {
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = 1,
            SwitchDate = new DateTime(2024, 6, 1),
            PreviousModel = SalaryModel.Static,
            NewModel = SalaryModel.InvoiceBased
        });
        await _context.SaveChangesAsync();

        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.Static);

        Assert.True(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(3, history.Count);
        var pending = history.Single(h => h.SwitchDate.Date > new DateTime(2026, 5, 28));
        Assert.Equal(SalaryModel.Static, pending.NewModel);
        Assert.Equal(SalaryModel.InvoiceBased, pending.PreviousModel);
        Assert.Equal(new DateTime(2026, 6, 1), pending.SwitchDate);
    }

    [Fact]
    public async Task UpdateSalaryModel_SameModelAsCurrent_ReturnsError()
    {
        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);

        var result = await userService.UpdateSalaryModel(1, SalaryModel.Static);

        Assert.False(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Single(history);
    }

    [Fact]
    public async Task CancelPendingSalaryModelChange_PendingExists_RemovesEntry()
    {
        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);
        await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        var result = await userService.CancelPendingSalaryModelChange(1);

        Assert.True(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Single(history);
    }

    [Fact]
    public async Task CancelPendingSalaryModelChange_NoPending_ReturnsError()
    {
        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);

        var result = await userService.CancelPendingSalaryModelChange(1);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserById_PendingExists_PopulatesPendingAndKeepsCurrentModelEffective()
    {
        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);
        await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        var user = await userService.GetUserById(1);

        Assert.NotNull(user.PendingSalaryModelChange);
        Assert.Equal(new DateTime(2026, 6, 1), user.PendingSalaryModelChange.EffectiveDate);
        Assert.Equal(SalaryModel.InvoiceBased, user.PendingSalaryModelChange.NewModel);
        Assert.Equal(SalaryModel.Static, user.SalaryModel);
        Assert.Empty(user.SalaryModelHistory);
    }

    [Fact]
    public async Task GetUserById_PastSwitchExists_ComputesCurrentEffectiveFromHistory()
    {
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = 1,
            SwitchDate = new DateTime(2024, 6, 1),
            PreviousModel = SalaryModel.Static,
            NewModel = SalaryModel.InvoiceBased
        });
        await _context.SaveChangesAsync();

        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);

        var user = await userService.GetUserById(1);

        Assert.Equal(SalaryModel.InvoiceBased, user.SalaryModel);
        Assert.Equal(new DateTime(2024, 6, 1), user.SalaryModelHistory[^1]!.SwitchDate);
        Assert.Single(user.SalaryModelHistory);
        Assert.Null(user.PendingSalaryModelChange);
    }

    [Fact]
    public async Task CancelPendingSalaryModelChange_DoesNotTouchPastEntries()
    {
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = 1,
            SwitchDate = new DateTime(2024, 6, 1),
            PreviousModel = SalaryModel.InvoiceBased,
            NewModel = SalaryModel.Static
        });
        await _context.SaveChangesAsync();

        var clock = CreateClock(new DateTime(2026, 5, 28));
        var userService = CreateUserService(clock);
        await userService.UpdateSalaryModel(1, SalaryModel.InvoiceBased);

        var result = await userService.CancelPendingSalaryModelChange(1);

        Assert.True(result.IsSuccess);
        var history = (await userService.GetSalaryModelHistory(1)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(new DateTime(2024, 6, 1), history[1].SwitchDate);
    }

    private static DateAlvTime CreateClock(DateTime today)
    {
        return new DateAlvTime { Provider = new TestDateAlvTimeProvider { OverriddenValue = today } };
    }

    private UserService CreateUserService()
    {
        return new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context));
    }

    private UserService CreateUserService(DateAlvTime dateAlvTime)
    {
        return new UserService(new UserRepository(_context, dateAlvTime), new TimeRegistrationStorage(_context), dateAlvTime);
    }
}