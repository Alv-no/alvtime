using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using User = AlvTime.Business.Users.User;

namespace AlvTime.Persistence.Repositories
{
    public class AccessTokenStorage : IAccessTokenStorage
    {
        private readonly AlvTime_dbContext _dbContext;

        public AccessTokenStorage(AlvTime_dbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AccessTokenDto> CreateLifetimeToken(PersonalAccessToken token)
        {
            var accessToken = new AccessTokens
            {
                UserId = token.User.Id,
                Value = token.Value,
                ExpiryDate = token.ExpiryDate,
                FriendlyName = token.FriendlyName,
            };

            _dbContext.AccessTokens.Add(accessToken);
            await _dbContext.SaveChangesAsync();

            return new AccessTokenDto(accessToken.Id, accessToken.Value, accessToken.ExpiryDate, accessToken.FriendlyName);
        }

        public async Task<AccessTokenDto> DeleteActiveToken(int tokenId)
        {
            var token = _dbContext.AccessTokens
                .FirstOrDefault(t => t.Id == tokenId);

            token.ExpiryDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return new AccessTokenDto(token.Id, token.Value, token.ExpiryDate, token.FriendlyName);
        }

        public async Task<IEnumerable<AccessTokenDto>> GetActiveTokens(User user)
        {
            var alvUser = _dbContext.User.First(dbUser => dbUser.Email.ToLower().Equals(user.Email.ToLower()));

            var tokens = await _dbContext.AccessTokens
                .Where(token => token.UserId == alvUser.Id && token.ExpiryDate >= DateTime.UtcNow)
                .ToListAsync();

            return tokens.Select(token =>
                new AccessTokenDto(token.Id, token.Value, token.ExpiryDate, token.FriendlyName));
        }
    }
}
