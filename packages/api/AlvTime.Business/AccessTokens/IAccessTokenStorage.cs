using System.Collections.Generic;
using AlvTime.Business.Models;

namespace AlvTime.Business.AccessTokens
{
    public interface IAccessTokenStorage
    {
        IEnumerable<AccessTokenDto> GetActiveTokens(User user);
        AccessTokenDto DeleteActiveTokens(int tokenId);
        AccessTokenDto CreateLifetimeToken(PersonalAccessToken token);
    }
}
