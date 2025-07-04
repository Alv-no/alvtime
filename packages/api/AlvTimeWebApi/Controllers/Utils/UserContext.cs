using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AlvTime.Business.Users;
using Microsoft.AspNetCore.Http;

namespace AlvTimeWebApi.Controllers.Utils;

public class UserContext(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    : IUserContext
{
    private string Name => httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

    private string Email => httpContextAccessor.HttpContext.User.FindFirstValue("preferred_username");

    private string Oid => httpContextAccessor.HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

    public async Task<User> GetCurrentUser()
    {
        if (string.IsNullOrEmpty(Oid))
        {
            throw new ValidationException("Bruker eksisterer ikke");
        }
            
        var dbUser = (await userRepository.GetUsers(new UserQuerySearch { Oid = Oid })).First();

        return UserMapper.MapUserDtoToBusinessUser(dbUser);
    }
}