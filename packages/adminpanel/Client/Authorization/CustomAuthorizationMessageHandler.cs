using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Alvtime.Adminpanel.Client.Authorization;

public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(IAccessTokenProvider provider, 
        NavigationManager navigation, IConfiguration configuration)
        : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: new[] { configuration["ApiSettings:BaseUrl"] }!);
    }
}