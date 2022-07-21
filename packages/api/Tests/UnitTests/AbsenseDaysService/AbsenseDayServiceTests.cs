using System;
using AlvTime.Business.Absence;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.AbsenseDaysService;

public class AbsenceDayStorageTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;

    public AbsenceDayStorageTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithUsers()
            .WithTasks()
            .WithLeaveTasks()
            .WithTimeEntries()
            .WithHourRates()
            .CreateDbContext();

        var entryOptions = new TimeEntryOptions
        {
            SickDaysTask = 14,
            PaidHolidayTask = 13,
            UnpaidHolidayTask = 12
        };
        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
            
        _userContextMock = new Mock<IUserContext>();

        var user = new AlvTime.Business.Models.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone"
        };

        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user);
    }

    [Fact]
    public void CheckRemainingHolidays_NoWithDrawls()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);
        var days = absenseService.GetAbsenceDays(1, 2020, null);

        Assert.Equal(0, days.UsedAbsenceDays);
    }

    [Fact]
    public void CheckRemainingHolidays_SickdaysTaken()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        // One day of sick leave
        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-2),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);

        var days = absenseService.GetAbsenceDays(1, 2021, null);

        Assert.Equal(3, days.UsedAbsenceDays);


        // These two withdrawals of sick days should also count as 3 as they are concurrent
    }

    [Fact]
    public void TestConcurrentDays()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-9),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-10),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);

        var days = absenseService.GetAbsenceDays(1, 2021, null);

        Assert.Equal(3, days.UsedAbsenceDays);
    }

    [Fact]
    public void TestConcurrentDays_MoreThanThree()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-9),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-10),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-11),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-12),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);

        var days = absenseService.GetAbsenceDays(1, 2021, null);

        // We withdraw three whole days
        Assert.Equal(6, days.UsedAbsenceDays);
    }
    
    [Fact]
    public void GetVacationDays_StartedNovember2019Used4DaysIn2020And20DaysIn2021_Has30DaysIn2022()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(2019, 11, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        for (int i = 3; i < 7; i++)
        {
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2020, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }

        for (int i = 1; i <= 26; i++)
        {
            var date = new DateTime(2021, 2, i);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }

        var holidayOverview = absenseService.GetAllTimeVacationOverview(2022);

        Assert.Equal(30, holidayOverview.AvailableVacationDays);
        Assert.Equal(0, holidayOverview.UsedVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_StartedJanuary2YearsAgoRecordedNoVacation_Has50Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.AddYears(-2).Year, 01, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);
        
        timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = new DateTime(DateTime.Now.AddYears(-2).Year, 12, 24),
            Value = 7.5M,
            TaskId = _options.CurrentValue.PaidHolidayTask
        }, 2);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(50, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_StartedJanuary2YearsAgoRecorded1VacationOnRedDay_Has50Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.AddYears(-2).Year, 01, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(50, holidayOverview.AvailableVacationDays);
        Assert.Equal(0, holidayOverview.UsedVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_StartedJulyLastYear_Has12Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.AddYears(-1).Year, 07, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(13, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_StartedThisYear_Has0Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.Year, 01, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(0, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_Started2ndJanuaryLastYear_Has25Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.Year-1, 01, 02)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(25, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_ThisYear_Has0Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.Year, 01, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(0, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_StartedJulyThisYear_Has0Days()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.Year, 07, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(0, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public void GetVacationDays_UserSpentMoreVacationThanAvailable_DoesNotHaveNegativeDays()
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = new DateTime(DateTime.Now.Year - 1, 01, 01)
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var limit = 20;
        for (int i = 1; i <= limit; i++)
        {
            var date = new DateTime(DateTime.Now.Year - 1, 2, i);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                limit++;
                continue;
            }
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(DateTime.Now.Year - 1, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }
        
        var limit2 = 20;
        for (int i = 1; i <= limit2; i++)
        {
            var date = new DateTime(DateTime.Now.Year, 2, i);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                limit2++;
                continue;
            }
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(DateTime.Now.Year, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }

        var holidayOverview = absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(5, holidayOverview.AvailableVacationDays);
    }

    private TimeRegistrationStorage CreateTimeRegistrationStorage()
    {
        return new TimeRegistrationStorage(_context);
    }

    private AbsenceDaysService CreateAbsenseDaysService(TimeRegistrationStorage storage)
    {
        return new AbsenceDaysService(storage, _options, _userContextMock.Object);
    } 
}