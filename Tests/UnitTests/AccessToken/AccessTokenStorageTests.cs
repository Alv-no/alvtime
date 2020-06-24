using AlvTime.Persistence.Repositories;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.AccessToken
{
    public class AccessTokenStorageTests
    {
        [Fact]
        public void GetActiveAccessTokens_UserSpecified_ActiveTokensForUser()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new AccessTokenStorage(context);

            var tokens = storage.GetActiveTokens(1);

            Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void CreateLifetimeToken_FriendlyNameSpecified_TokenWithFriendlyNameCreated()
        {
            var context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new AccessTokenStorage(context);

            storage.CreateLifetimeToken("new token", 1);

            var tokens = storage.GetActiveTokens(1);

            Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void DeleteToken_TokenIdSpecified_TokenWithIdDeleted()
        {
            var context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new AccessTokenStorage(context);

            storage.DeleteActiveTokens(1, 1);

            var tokens = storage.GetActiveTokens(1);

            Assert.Empty(tokens);
        }
    }
}
