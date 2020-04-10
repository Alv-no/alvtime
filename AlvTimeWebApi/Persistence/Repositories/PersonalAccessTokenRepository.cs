using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Business;
using AlvTimeWebApi.Business.PersonalAccessToken;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Persistence.Repositories
{
    public class PersonalAccessTokenRepository : IPersonalAccessTokenStorage
    {
        public Task<User> GetUserFromToken(Token token)
        {
            if (token.Value != "1234")
            {
                return Task.FromResult((User)null);
            }

            return Task.FromResult(new User
            {
                Name = "Some One",
                Email = "someone@alv.no"
            });
        }
    }
}
