using AlvTime.Persistence.Repositories;
using System.Linq;
using System.Globalization;
using System;
using Xunit;

namespace Tests.UnitTests.AccessToken
{
    public class AccessTokenStorageTests
    {
        [Fact]
        public void GetActiveAccessTokens_UserSpecified_ActiveTokensForUser()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var storage = new AccessTokenStorage(context);

            var tokens = storage.GetActiveTokens(1);

            Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void CreateLifetimeToken_FriendlyNameSpecified_TokenWithFriendlyNameCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithPersonalAccessTokens()
                .WithUsers()
                .CreateDbContext();

            var storage = new AccessTokenStorage(context);

            storage.CreateLifetimeToken("new token", 1);

            var tokens = storage.GetActiveTokens(1);

            Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void DeleteToken_TokenIdSpecified_TokenWithIdDeleted()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithPersonalAccessTokens()
                .WithUsers()
                .CreateDbContext();

            var storage = new AccessTokenStorage(context);

            var oldExpiryDate = context.AccessTokens.ToList().First().ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            storage.DeleteActiveTokens(1, 1);

            var newExpiryDate = storage.GetActiveTokens(1).Where(x => x.Id == 1).ToList().First().ExpiryDate;
            Assert.NotEqual(newExpiryDate, oldExpiryDate);
        }
    }
}
