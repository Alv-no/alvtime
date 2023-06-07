using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.AccessTokens
{
    public interface IAccessTokenStorage
    {
        Task<IEnumerable<AccessTokenDto>> GetActiveTokens(User user);
        Task<AccessTokenDto> DeleteActiveTokens(int tokenId);
        Task<AccessTokenDto> CreateLifetimeToken(PersonalAccessToken token);
    }
}
