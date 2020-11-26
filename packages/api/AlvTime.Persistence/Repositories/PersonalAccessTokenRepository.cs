using AlvTime.Business.AccessToken.PersonalAccessToken;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Persistence.Repositories
{
    public class PersonalAccessTokenRepository : IPersonalAccessTokenStorage
    {
        private readonly AlvTime_dbContext _database;

        public PersonalAccessTokenRepository(AlvTime_dbContext database)
        {
            _database = database;
        }

        public async Task<AlvTime.Business.AccessToken.User> GetUserFromToken(Token token)
        {
            var databaseToken = await _database.AccessTokens.FirstOrDefaultAsync(x => x.Value == token.Value && x.ExpiryDate >= DateTime.UtcNow);

            if(databaseToken != null)
            {
                var databaseUser = await _database.User.FirstOrDefaultAsync(x => x.Id == databaseToken.UserId);

                return new AlvTime.Business.AccessToken.User
                {
                    Id = databaseUser.Id,
                    Email = databaseUser.Email,
                    Name = databaseUser.Name
                };
            }

            return null;
        }
    }
}
