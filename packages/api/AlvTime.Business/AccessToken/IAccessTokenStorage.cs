using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.AccessToken
{
    public interface IAccessTokenStorage
    {
        IEnumerable<AccessTokenFriendlyNameResponseDto> GetActiveTokens(int userId);
        AccessTokenFriendlyNameResponseDto DeleteActiveTokens(int tokenId, int userId);
        AccessTokenResponseDto CreateLifetimeToken(string friendlyName, int userId);
    }
}
