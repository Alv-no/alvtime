using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;

namespace AlvTime.Business.AccessTokens
{
    public class AccessTokenService
    {
        private readonly IUserContext _userContext;
        private readonly IAccessTokenStorage _tokenStorage;

        public AccessTokenService(IAccessTokenStorage tokenStorage, IUserContext userContext)
        {
            _tokenStorage = tokenStorage;
            _userContext = userContext;
        }

        public AccessTokenDto CreateLifeTimeToken(string friendlyName)
        {
            var currentUser = _userContext.GetCurrentUser();
            var personalAccessToken = new PersonalAccessToken
            {
                User = currentUser,
                Value = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.Now.AddMonths(6),
                FriendlyName = friendlyName
            };

            return _tokenStorage.CreateLifetimeToken(personalAccessToken);
        }

        public IEnumerable<AccessTokenDto> DeleteActiveTokens(IEnumerable<int> tokenIds)
        {
            var userTokenIds = GetActiveTokens().Select(token => token.Id);

            var response = new List<AccessTokenDto>();

            foreach (var tokenId in tokenIds)
            {
                if (userTokenIds.Contains(tokenId))
                {
                    response.Add(_tokenStorage.DeleteActiveTokens(tokenId));
                }
            }

            return response;
        }

        public IEnumerable<AccessTokenDto> GetActiveTokens()
        {
            var currentUser = _userContext.GetCurrentUser();

            return _tokenStorage.GetActiveTokens(currentUser);
        }
    }
}