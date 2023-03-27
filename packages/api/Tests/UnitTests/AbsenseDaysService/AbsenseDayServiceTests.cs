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
using Task = System.Threading.Tasks.Task;

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

        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(Task.FromResult(user));
    }

    [Fact]
    public async Task CheckRemainingHolidays_NoWithDrawls()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);
        var days = await absenseService.GetAbsenceDays(1, 2020, null);

        Assert.Equal(0, days.UsedAbsenceDays);
    }

    [Fact]
    public async Task CheckRemainingHolidays_SickdaysTaken()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        // One day of sick leave
        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-2),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);

        var days = await absenseService.GetAbsenceDays(1, 2021, null);

        Assert.Equal(3, days.UsedAbsenceDays);


        // These two withdrawals of sick days should also count as 3 as they are concurrent
    }

    [Fact]
    public async Task TestConcurrentDays()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-9),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-10),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);

        var days = await absenseService.GetAbsenceDays(1, 2021, null);

        Assert.Equal(3, days.UsedAbsenceDays);
    }

    [Fact]
    public async Task TestConcurrentDays_MoreThanThree()
    {
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-9),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-10),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-11),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);
        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = DateTime.Now.AddDays(-12),
            Value = 5,
            TaskId = _options.CurrentValue.SickDaysTask
        }, 1);

        var days = await absenseService.GetAbsenceDays(1, 2021, null);

        // We withdraw three whole days
        Assert.Equal(6, days.UsedAbsenceDays);
    }
    
    [Fact]
    public async Task GetVacationDays_StartedNovember2019Used4DaysIn2020And20DaysIn2021_Has30DaysIn2022()
    {
        CreateUserWithStartDate(new DateTime(2019, 11, 01));

        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        for (int i = 3; i < 7; i++)
        {
            await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
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
            await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(2022);

        Assert.Equal(30, holidayOverview.AvailableVacationDays);
        Assert.Equal(0, holidayOverview.UsedVacationDaysThisYear);
    }

    private void CreateUserWithStartDate(DateTime date)
    {
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else",
            StartDate = date
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(Task.FromResult(user2));
    }

    [Fact]
    public async Task GetVacationDays_StartedJanuary2YearsAgoRecordedNoVacation_Has50Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.AddYears(-2).Year, 01, 01));
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);
        
        await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = new DateTime(DateTime.Now.AddYears(-2).Year, 12, 24),
            Value = 7.5M,
            TaskId = _options.CurrentValue.PaidHolidayTask
        }, 2);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(50, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_StartedJanuary2YearsAgoRecorded1VacationOnRedDay_Has50Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.AddYears(-2).Year, 01, 01));
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(50, holidayOverview.AvailableVacationDays);
        Assert.Equal(0, holidayOverview.UsedVacationDaysThisYear);
    }
    
    [Fact]
    public async Task GetVacationDays_StartedJulyLastYear_Has12Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.AddYears(-1).Year, 07, 01));
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(13, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_StartedThisYear_Has0Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year, 01, 01));
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(0, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_Started2ndJanuaryLastYear_Has25Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year-1, 01, 02));
        
        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(25, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_ThisYear_Has0Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year, 01, 01));

        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(0, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_StartedJulyThisYear_Has0Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year, 07, 01));

        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(0, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_UserSpentMoreVacationThanAvailable_DoesNotHaveNegativeDays()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year - 1, 01, 01));

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
            await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
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
            await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(DateTime.Now.Year, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(5, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_GraduateStartedTwoYearsAgoAndPaidFor15VacationDaysLastYear_Has25VacationDays()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year - 2, 08, 01));
        _context.VacationDaysEarnedOverride.Add(new VacationDaysEarnedOverride
        {
            UserId = 2,
            Year = DateTime.Now.Year - 1,
            DaysEarned = 15
        });
        await _context.SaveChangesAsync();

        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(25, holidayOverview.AvailableVacationDays);
    }
    
    //Tests that using more days than generated is still supported
    [Fact]
    public async Task GetVacationDays_GraduateStartedTwoYearsAgoAndPaidFor15VacationDaysLastYearAndUsed15DaysLastYear_Has15VacationDays()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year - 2, 08, 01));
        
        _context.VacationDaysEarnedOverride.Add(new VacationDaysEarnedOverride
        {
            UserId = 2,
            Year = DateTime.Now.Year - 1,
            DaysEarned = 15
        });
        await _context.SaveChangesAsync();

        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);
        
        var limit = 15;
        for (int i = 1; i <= limit; i++)
        {
            var date = new DateTime(DateTime.Now.Year - 1, 2, i);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                limit++;
                continue;
            }
            await timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(DateTime.Now.Year - 1, 2, i),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 2);
        }

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(15, holidayOverview.AvailableVacationDays);
    }
    
    [Fact]
    public async Task GetVacationDays_GraduateStartedLastYearAndPaidFor25DaysLastYear_Has25Days()
    {
        CreateUserWithStartDate(new DateTime(DateTime.Now.Year - 1, 08, 01));
        _context.VacationDaysEarnedOverride.Add(new VacationDaysEarnedOverride
        {
            UserId = 2,
            Year = DateTime.Now.Year - 1,
            DaysEarned = 25
        });
        await _context.SaveChangesAsync();

        var timeRegistrationStorage = CreateTimeRegistrationStorage();
        var absenseService = CreateAbsenseDaysService(timeRegistrationStorage);

        var holidayOverview = await absenseService.GetAllTimeVacationOverview(DateTime.Now.Year);

        Assert.Equal(25, holidayOverview.AvailableVacationDays);
    }

    private TimeRegistrationStorage CreateTimeRegistrationStorage()
    {
        return new TimeRegistrationStorage(_context);
    }

    private AbsenceDaysService CreateAbsenseDaysService(TimeRegistrationStorage storage)
    {
        return new AbsenceDaysService(storage, _options, _userContextMock.Object, new AbsenceStorage(_context));
    } 
}