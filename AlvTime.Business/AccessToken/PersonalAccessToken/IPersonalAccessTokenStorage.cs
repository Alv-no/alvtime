using AlvTimeWebApi.Business;
using AlvTimeWebApi.Business.PersonalAccessToken;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Authentication
{
    public interface IPersonalAccessTokenStorage
    {
        Task<User> GetUserFromToken(Token token);
    }
}