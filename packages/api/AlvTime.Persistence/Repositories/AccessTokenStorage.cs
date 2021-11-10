﻿using AlvTime.Persistence.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.AccessTokens;
using User = AlvTime.Business.Models.User;

namespace AlvTime.Persistence.Repositories
{
    public class AccessTokenStorage : IAccessTokenStorage
    {
        private readonly AlvTime_dbContext _dbContext;

        public AccessTokenStorage(AlvTime_dbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AccessTokenDto CreateLifetimeToken(PersonalAccessToken token)
        {
            var accessToken = new AccessTokens
            {
                UserId = token.User.Id,
                Value = token.Value,
                ExpiryDate = token.ExpiryDate,
                FriendlyName = token.FriendlyName,
            };

            _dbContext.AccessTokens.Add(accessToken);
            _dbContext.SaveChanges();

            return new AccessTokenDto(accessToken.Id, accessToken.Value, accessToken.ExpiryDate, accessToken.FriendlyName);
        }

        public AccessTokenDto DeleteActiveTokens(int tokenId)
        {
            var token = _dbContext.AccessTokens
                .FirstOrDefault(t => t.Id == tokenId);

            token.ExpiryDate = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return new AccessTokenDto(token.Id, token.Value, token.ExpiryDate, token.FriendlyName);
        }

        public IEnumerable<AccessTokenDto> GetActiveTokens(User user)
        {
            var alvUser = _dbContext.User.First(dbUser => dbUser.Email.ToLower().Equals(user.Email.ToLower()));

            var tokens = _dbContext.AccessTokens
                .Where(token => token.UserId == alvUser.Id && token.ExpiryDate >= DateTime.UtcNow)
                .ToList();

            return tokens.Select(token =>
                new AccessTokenDto(token.Id, token.Value, token.ExpiryDate, token.FriendlyName));
        }
    }
}
