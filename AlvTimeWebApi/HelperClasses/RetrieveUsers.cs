using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace AlvTimeWebApi.HelperClasses
{
    public class RetrieveUsers
    {
        private readonly AlvTime_dbContext _database;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RetrieveUsers(AlvTime_dbContext database, IHttpContextAccessor httpContextAccessor)
        {
            _database = database;
            _httpContextAccessor = httpContextAccessor;
        }

        public User RetrieveUser()
        {
            var username = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var email = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var alvUser = _database.User.FirstOrDefault(x => x.Email.Equals(email));

            return alvUser;
        }
    }
}
