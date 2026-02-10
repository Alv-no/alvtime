using System.Collections.Generic;
using AlvTime.Persistence.Repositories;
using System.Linq;
using AlvTime.Business.AccessTokens;
using AlvTime.Persistence.DatabaseModels;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;
using User = AlvTime.Business.Users.User;
using AlvTime.Business.Users;

namespace Tests.UnitTests.AccessToken;

public class AccessTokenServiceTests
{
    private readonly AlvTime_dbContext _context;
    
    public AccessTokenServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithPersonalAccessTokens()
            .WithUsers()
            .CreateDbContext();
    }
    
    [Fact]
    public async Task GetActiveAccessTokens_UserSpecified_ActiveTokensForUser()
    {
        var service = CreateAccessTokenService(_context);

        var tokens = await service.GetActiveTokens();

        Assert.Equal(_context.AccessTokens.Where(x => x.UserId == 1).ToList().Count, tokens.Count());
    }

    [Fact]
    public async Task CreateLifetimeToken_FriendlyNameSpecified_TokenWithFriendlyNameCreated()
    {
        var service = CreateAccessTokenService(_context);

        await service.CreateLifeTimeToken("new token");

        var tokens = (await service.GetActiveTokens()).ToList();

        Assert.Equal(2, tokens.Count);
        Assert.Equal("new token", tokens.Last().FriendlyName);
    }

    [Fact]
    public async Task DeleteToken_TokenIdSpecified_TokenWithIdDeleted()
    {
        var service = CreateAccessTokenService(_context);

        await service.DeleteToken(1);

        var tokens = await service.GetActiveTokens();

        Assert.Empty(tokens);
    }
    
    private static AccessTokenService CreateAccessTokenService(AlvTime_dbContext dbContext)
    {
        var mockUserContext = new Mock<IUserContext>();

        var user = new User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone",
            Oid = "12345678-1234-1234-1234-123456789012"
        };

        mockUserContext.Setup(context => context.GetCurrentUser()).Returns(Task.FromResult(user));
            
        return new AccessTokenService(new AccessTokenStorage(dbContext), mockUserContext.Object);
    }
}