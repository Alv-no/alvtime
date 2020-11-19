using AlvTime.Business.AccessToken;
using AlvTime.Persistence.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public class AccessTokenStorage : IAccessTokenStorage
    {
        private readonly AlvTime_dbContext _context;

        public AccessTokenStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public AccessTokenResponseDto CreateLifetimeToken(string friendlyName, int userId)
        {
            var uuid = Guid.NewGuid().ToString();

            var accessToken = new AccessTokens
            {
                UserId = userId,
                Value = uuid,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                FriendlyName = friendlyName
            };

            _context.AccessTokens.Add(accessToken);
            _context.SaveChanges();

            return new AccessTokenResponseDto
            {
                Token = uuid,
                ExpiryDate = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        public AccessTokenFriendlyNameResponseDto DeleteActiveTokens(int tokenId, int userId)
        {
            var token = _context.AccessTokens
                .FirstOrDefault(t => t.Id == tokenId && t.UserId == userId);

            token.ExpiryDate = DateTime.UtcNow;
            _context.SaveChanges();

            return new AccessTokenFriendlyNameResponseDto
            {
                Id = token.Id,
                FriendlyName = token.FriendlyName,
                ExpiryDate = token.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        public IEnumerable<AccessTokenFriendlyNameResponseDto> GetActiveTokens(int userId)
        {
            return _context.AccessTokens
                .Where(x => x.UserId == userId && x.ExpiryDate >= DateTime.UtcNow)
                .Select(x => new AccessTokenFriendlyNameResponseDto
                {
                    Id = x.Id,
                    FriendlyName = x.FriendlyName,
                    ExpiryDate = x.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                })
                .ToList();
        }
    }
}
