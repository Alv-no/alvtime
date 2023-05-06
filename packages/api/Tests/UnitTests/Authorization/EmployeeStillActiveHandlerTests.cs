using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Models;
using AlvTime.Business.Users;
using AlvTime.Persistence.Repositories;
using AlvTimeWebApi.Authentication.PersonalAccessToken;
using AlvTimeWebApi.Authorization.Handlers;
using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
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
            EndDate = DateTime.Now.AddMonths(-1)
        });

        var accessToken = await dbContext.AccessTokens.FindAsync(1);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken?.Value}");

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[] {
                    new("preferred_username", "someone@alv.no"),
                },
                "Basic")
        );

        var requirements = new[] { new EmployeeStillActiveRequirement() };
        var context = new AuthorizationHandlerContext(requirements, user, null);
        var handler = new EmployeeStillActiveHandler(dbContext);

        await handler.HandleAsync(context);
        Assert.True(context.HasFailed);
        Assert.Equal("Employee is no longer active", context.FailureReasons.First().Message);
    }
}