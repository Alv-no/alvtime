using AlvTimeWebApi.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Controllers;

[Authorize]
[Route("api/auth")]
public class AuthController : Controller
{
    [AllowAnonymous]
    [HttpGet("login")]
    public async Task Login(string returnUrl = "/")
    {
        await HttpContext.ChallengeAsync(new AuthenticationProperties
        {
            RedirectUri = returnUrl
        });
    }

    [HttpGet("logout")]
    public async Task Logout(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync("AzureAd", new AuthenticationProperties
        {
            RedirectUri = returnUrl
        });
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
    
    [AllowAnonymous]
    [HttpGet("userinfo")]
    public async Task<UserInfo> GetUserInfo()
    {
        var user = await HttpContext.AuthenticateAsync();
        return new UserInfo
        {
            IsAuthenticated = user.Succeeded,
            Name = user.Principal?.FindFirst("name")?.Value,
            Roles = user.Principal?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? []
        };
    }
}