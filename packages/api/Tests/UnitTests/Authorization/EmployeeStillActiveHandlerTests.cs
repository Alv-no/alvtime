using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AlvTime.Business.Users;
using AlvTime.Persistence.Repositories;
using AlvTimeWebApi.Authorization.Handlers;
using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Tests.UnitTests.Authorization;

public class EmployeeStillActiveHandlerTests
{
    [Fact]
    public async Task Authenticate_ExpiredUser_WithValidPAT_ShouldFail()
    {
        // Arrange
        var dbContext = new AlvTimeDbContextBuilder()
            .WithPersonalAccessTokens()
            .WithUsers()
            .CreateDbContext();

        var userStorage = new UserRepository(dbContext);

        // This user and the following access token are provided by the WithUsers|WithPATs methods above
        await userStorage.UpdateUser(new UserDto
        {
            Id = 1,
            EndDate = DateTime.Now.AddMonths(-1),
            Oid = "12345678-1234-1234-1234-123456789012"
        });

        var accessToken = await dbContext.AccessTokens.FindAsync(1);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {accessToken?.Value}");

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[] {
                    new("preferred_username", "someone@alv.no"),
                },
                "Basic")
        );

        var requirements = new[] { new EmployeeStillActiveRequirement() };
        var context = new AuthorizationHandlerContext(requirements, user, null);
        var handler = new EmployeeIsActiveHandler(dbContext);

        await handler.HandleAsync(context);
        Assert.True(context.HasFailed);
        Assert.Equal("Employee is no longer active", context.FailureReasons.First().Message);
    }

    [Fact]
    public async Task Authenticate_NotStartedUser_WithValidPAT_ShouldFail()
    {
        // Arrange
        var dbContext = new AlvTimeDbContextBuilder()
            .WithPersonalAccessTokens()
            .WithUsers()
            .CreateDbContext();

        var userStorage = new UserRepository(dbContext);

        // This user and the following access token are provided by the WithUsers|WithPATs methods above
        await userStorage.UpdateUser(new UserDto
        {
            Id = 1,
            StartDate = DateTime.Now.AddMonths(1),
            Oid = "12345678-1234-1234-1234-123456789012"
        });

        var accessToken = await dbContext.AccessTokens.FindAsync(1);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {accessToken?.Value}");

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim("preferred_username", "someone@alv.no"),
                    new Claim("oid", "12345678-1234-1234-1234-123456789012")
                ],
                "Basic")
        );

        var requirements = new[] { new EmployeeStillActiveRequirement() };
        var context = new AuthorizationHandlerContext(requirements, user, null);
        var handler = new EmployeeIsActiveHandler(dbContext);

        await handler.HandleAsync(context);
        Assert.True(context.HasFailed);
        Assert.Equal("Employee is not active yet", context.FailureReasons.First().Message);
    }
}