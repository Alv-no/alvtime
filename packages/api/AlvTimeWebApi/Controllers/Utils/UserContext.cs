using System;
using System.Linq;
using System.Security.Claims;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Models;
using AlvTime.Business.Users;
using Microsoft.AspNetCore.Http;

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

        private string Name => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

        private string Email => _httpContextAccessor.HttpContext.User.FindFirstValue("preferred_username");

        public User GetCurrentUser()
        {
            var dbUser = _userStorage.GetUser(new UserQuerySearch { Email = Email }).First();

            return new User { Id = dbUser.Id, Email = Email, Name = Name, StartDate = DateTime.Parse(dbUser.StartDate) };
        }
    }
}