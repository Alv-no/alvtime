using AlvTime.Persistence.Repositories;
using System.Linq;
using System;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Models;
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

            var user = new User
            {
                Name = context.User.First().Name, 
                Email = context.User.First().Email
            };
            
            var tokens = storage.GetActiveTokens(user);

            Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void CreateLifetimeToken_FriendlyNameSpecified_TokenWithFriendlyNameCreated()
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

            storage.CreateLifetimeToken(token);

            var tokens = storage.GetActiveTokens(user);

            Assert.Equal(context.AccessTokens.Where(x => x.UserId == 1).ToList().Count(), tokens.Count());
        }

        [Fact]
        public void DeleteToken_TokenIdSpecified_TokenWithIdDeleted()
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

            storage.DeleteActiveTokens(1);

            var tokens = storage.GetActiveTokens(user);
            Assert.Empty(tokens);
        }
    }
}
