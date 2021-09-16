using System;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.AspNetCore.Http;
using System.Linq;
using AlvTimeWebApi.Exceptions;

namespace AlvTimeWebApi.Controllers.Utils
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
                var email = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
                var alvUser = _database.User.FirstOrDefault(user => user.Email.ToLower().Equals(email.ToLower()));

                if (alvUser?.EndDate != null && alvUser.EndDate <= DateTime.Now)
                    throw new AuthorizationException("Your account has been deactivated.");

                return alvUser;
        }
    }
}
