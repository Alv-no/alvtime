using System.Threading.Tasks;

namespace AlvTime.Business.AccessToken.PersonalAccessToken
{
    public interface IPersonalAccessTokenStorage
    {
        Task<User> GetUserFromToken(Token token);
    }
}