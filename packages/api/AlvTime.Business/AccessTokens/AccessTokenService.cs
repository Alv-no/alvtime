using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Users;

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

        public async Task<AccessTokenDto> CreateLifeTimeToken(string friendlyName)
        {
            var currentUser = await _userContext.GetCurrentUser();
            var personalAccessToken = new PersonalAccessToken
            {
                User = currentUser,
                Value = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.Now.AddMonths(6),
                FriendlyName = friendlyName
            };

            return await _tokenStorage.CreateLifetimeToken(personalAccessToken);
        }

        public async Task DeleteToken(int tokenId)
        {
            var userTokenIds = (await GetActiveTokens()).Select(token => token.Id);
            
            if (userTokenIds.Contains(tokenId))
            {
                await _tokenStorage.DeleteActiveToken(tokenId);
            }
        }

        public async Task<IEnumerable<AccessTokenDto>> GetActiveTokens()
        {
            var currentUser = await _userContext.GetCurrentUser();

            return await _tokenStorage.GetActiveTokens(currentUser);
        }
    }
}