using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Models;
using AlvTime.Business.Users;
using AlvTime.Persistence.Repositories;
using AlvTimeWebApi.Authentication.PersonalAccessToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Tests.UnitTests.AccessToken;

public class AccessTokenStorageTests
{
    [Fact]
    public async Task GetActiveAccessTokens_UserSpecified_ActiveTokensForUser()
    {
        var context = new AlvTimeDbContextBuilder()
            .WithUsers()
            .CreateDbContext();

        var storage = new AccessTokenStorage(context);

        var user = new User
        {
            Name = context.User.First().Name,
            Email = context.User.First().Email
        };

        var tokens = await storage.GetActiveTokens(user);

        Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
    }

    [Fact]
    public async Task CreateLifetimeToken_FriendlyNameSpecified_TokenWithFriendlyNameCreated()
    {
        var context = new AlvTimeDbContextBuilder()
            .WithPersonalAccessTokens()
            .WithUsers()
            .CreateDbContext();

        var user = new User
        {
            Name = context.User.First().Name,
            Email = context.User.First().Email
        };

        var storage = new AccessTokenStorage(context);

        var token = new PersonalAccessToken
        {
            User = user,
            Value = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.Now.AddMonths(6),
            FriendlyName = "new token"
        };

        await storage.CreateLifetimeToken(token);

        var tokens = await storage.GetActiveTokens(user);

        Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
    }

    [Fact]
    public async Task DeleteToken_TokenIdSpecified_TokenWithIdDeleted()
    {
        var context = new AlvTimeDbContextBuilder()
            .WithPersonalAccessTokens()
            .WithUsers()
            .CreateDbContext();

        var user = new User
        {
            Name = context.User.First().Name,
            Email = context.User.First().Email
        };

        var storage = new AccessTokenStorage(context);

        await storage.DeleteActiveTokens(1);

        var tokens = await storage.GetActiveTokens(user);
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task Authenticate_ExpiredUser_WithValidPAT_ShouldFail()
    {
        // Arrange
        var dbContext = new AlvTimeDbContextBuilder()
            .WithPersonalAccessTokens()
            .WithUsers()
            .CreateDbContext();

        var storage = new UserRepository(dbContext);

        // This user and the following access token are provided by the WithUsers|WithPATs methods above
        await storage.UpdateUser(new UserDto
        {
            Id = 1,
            EndDate = DateTime.Now.AddMonths(-1)
        });

        var accessToken = await dbContext.AccessTokens.FindAsync(1);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken?.Value}");

        // The options and loggerFactory objects must return these values. If not, we get a NullPtrException
        // Source: https://stackoverflow.com/questions/58963133/unit-test-custom-authenticationhandler-middleware
        var options = new Mock<IOptionsMonitor<PersonalAccessTokenOptions>>();
        options.Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new PersonalAccessTokenOptions());

        var logger = new Mock<ILogger<PersonalAccessTokenHandler>>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var authenticationHandler = new PersonalAccessTokenHandler(
            options.Object,
            loggerFactory.Object,
            UrlEncoder.Default,
            storage,
            Mock.Of<SystemClock>());

        await authenticationHandler.InitializeAsync(
            new AuthenticationScheme(nameof(PersonalAccessTokenHandler), null, typeof(PersonalAccessTokenHandler)),
            httpContext);

        // Act
        var result = await authenticationHandler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("The user has an end date in the past", result.Failure?.Message);
    }
}