using System;
using AlvTime.Business.Options;
using AlvTime.Business.Projects;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Projects;

public class ProjectServiceTests
{
    private readonly AlvTime_dbContext _context;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    
    public ProjectServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .CreateDbContext();
        
        _userContextMock = new Mock<IUserContext>();

        var user = new AlvTime.Business.Users.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone",
            Oid = "12345678-1234-1234-1234-123456789012"
        };

        _userContextMock.Setup(context => context.GetCurrentUser())
            .Returns(Task.FromResult(user));
        
        var entryOptions = new TimeEntryOptions
        {
            SickDaysTask = 14,
            PaidHolidayTask = 13,
            UnpaidHolidayTask = 19,
            FlexTask = 18,
            StartOfOvertimeSystem = new DateTime(2020, 01, 01),
            AbsenceProject = 9
        };
        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
    }
    
    [Fact]
    public async Task CreateProject_NameSpecified_CustomerWithNameIsCreated()
    {
        var projectService = CreateProjectService(_context);

        var created = (await projectService.CreateProject("Test", 1)).Value;

        Assert.Equal(1, created.Id);
    }

    private ProjectService CreateProjectService(AlvTime_dbContext context)
    {
        var storage = new ProjectStorage(context, CreateTaskUtils());
        return new ProjectService(storage, _userContextMock.Object);
    }
    
    private TaskUtils CreateTaskUtils()
    {
        return new TaskUtils(new TaskStorage(_context), _options);
    }
}