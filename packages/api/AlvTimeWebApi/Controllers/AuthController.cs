using System.Threading.Tasks;
using AlvTimeWebApi.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task Logout()
    {
        await HttpContext.SignOutAsync();
    }
    
    [AllowAnonymous]
    [HttpGet("userinfo")]
    public async Task<UserInfo> GetUserInfo()
    {
        var user = await HttpContext.AuthenticateAsync();
        return new UserInfo
        {
            IsAuthenticated = user.Succeeded,
            Name = user.Principal?.Identity?.Name,
            Email = user.Principal?.FindFirst("preferred_username")?.Value,
        };
    }
}