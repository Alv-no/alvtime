using System;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Users;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.AspNetCore.Http;
using User = AlvTime.Business.Models.User;

namespace AlvTimeWebApi.Controllers.Utils
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserStorage _userStorage;

        public UserContext(IHttpContextAccessor httpContextAccessor, IUserStorage userStorage)
        {
            _httpContextAccessor = httpContextAccessor;
            _userStorage = userStorage;
        }

        private string Name => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "name").Value;

        private string Email => _httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == "preferred_username").Value;

        public User GetCurrentUser()
        {
            var dbUser = _userStorage.GetUser(new UserQuerySearch { Email = Email }).First();

            return new User { Id = dbUser.Id, Email = Email, Name = Name, StartDate = DateTime.Parse(dbUser.StartDate) };
        }
    }
}