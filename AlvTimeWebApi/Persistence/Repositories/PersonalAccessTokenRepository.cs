using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Business.PersonalAccessToken;
using AlvTimeWebApi.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace AlvTimeWebApi.Persistence.Repositories
{
    public class PersonalAccessTokenRepository : IPersonalAccessTokenStorage
    {
        private readonly AlvTime_dbContext _database;

        public PersonalAccessTokenRepository(AlvTime_dbContext database)
        {
            _database = database;
        }

        public async Task<Business.User> GetUserFromToken(Token token)
        {
            var databaseToken = await _database.AccessTokens.FirstOrDefaultAsync(x => x.Value == token.Value && x.ExpiryDate >= DateTime.UtcNow);

            if(databaseToken != null)
            {
                var databaseUser = await _database.User.FirstOrDefaultAsync(x => x.Id == databaseToken.UserId);

                return new Business.User
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
