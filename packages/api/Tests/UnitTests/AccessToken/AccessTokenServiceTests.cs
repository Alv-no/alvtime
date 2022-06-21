using System.Collections.Generic;
using AlvTime.Persistence.Repositories;
using System.Linq;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Interfaces;
using AlvTime.Persistence.DatabaseModels;
using Moq;
using Xunit;
using User = AlvTime.Business.Models.User;

namespace Tests.UnitTests.AccessToken
{
    public class AccessTokenServiceTests
    {
        [Fact]
        public void GetActiveAccessTokens_UserSpecified_ActiveTokensForUser()
        {
            var dbContext = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var service = CreateAccessTokenService(dbContext);

            var tokens = service.GetActiveTokens();

            Assert.Equal(dbContext.AccessTokens.Where(x => x.UserId == 1).ToList().Count, tokens.Count());
        }

        [Fact]
        public void CreateLifetimeToken_FriendlyNameSpecified_TokenWithFriendlyNameCreated()
        {
            var dbContext = new AlvTimeDbContextBuilder()
                .WithPersonalAccessTokens()
                .WithUsers()
                .CreateDbContext();

            var service = CreateAccessTokenService(dbContext);

            service.CreateLifeTimeToken("new token");

            var tokens = service.GetActiveTokens();

            Assert.Equal(dbContext.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void DeleteToken_TokenIdSpecified_TokenWithIdDeleted()
        {
            var dbContext = new AlvTimeDbContextBuilder()
                .WithPersonalAccessTokens()
                .WithUsers()
                .CreateDbContext();

            var service = CreateAccessTokenService(dbContext);

            service.DeleteActiveTokens(new List<int>{1});

            var tokens = service.GetActiveTokens();

            Assert.Empty(tokens);
        }

        private static AccessTokenService CreateAccessTokenService(AlvTime_dbContext dbContext)
        {
            var mockUserContext = new Mock<IUserContext>();

            var user = new User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            mockUserContext.Setup(context => context.GetCurrentUser()).Returns(user);
            
            return new AccessTokenService(new AccessTokenStorage(dbContext), mockUserContext.Object);
        }
    }
}
