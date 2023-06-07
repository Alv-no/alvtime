using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Users;
using Microsoft.AspNetCore.Http;

namespace AlvTimeWebApi.Controllers.Utils
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public UserContext(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        private string Name => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

        private string Email => _httpContextAccessor.HttpContext.User.FindFirstValue("preferred_username");

        public async Task<User> GetCurrentUser()
        {
            if (string.IsNullOrEmpty(Email))
            {
                throw new ValidationException("Bruker eksisterer ikke");
            }
            
            var dbUser = (await _userRepository.GetUsers(new UserQuerySearch { Email = Email })).First();

            return UserMapper.MapUserDtoToBusinessUser(dbUser);
        }
    }
}