using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Alvtime.Adminpanel.Client.Authorization;

public class BffAuthenticationStateProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<BffAuthenticationStateProvider> logger,
    IConfiguration configuration)
    : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Alvtime.API");

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var apiUrl = configuration["ApiSettings:BaseUrl"];
            var userInfo = await _httpClient.GetFromJsonAsync<BffUserInfo>($"{apiUrl}/api/auth/userInfo");

            if (userInfo?.IsAuthenticated == true)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Name ?? "Unknown"),
                };

                if (userInfo.Roles != null)
                {
                    claims.AddRange(userInfo.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
                }

                var identity = new ClaimsIdentity(claims, "BffCookie");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get user info from BFF");
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}

public class BffUserInfo
{
    public bool IsAuthenticated { get; set; }
    public string? Name { get; set; }
    public string[]? Roles { get; set; }
}