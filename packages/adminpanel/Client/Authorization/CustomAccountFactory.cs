namespace Alvtime.Adminpanel.Client.Authorization;

using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Security.Claims;
using System.Text.Json;

//https://auth0.com/blog/role-based-access-control-in-blazor-apps/
public class CustomAccountFactory(IAccessTokenProviderAccessor accessor) : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var userAccount = await base.CreateUserAsync(account, options);
        var userIdentity = userAccount.Identity as ClaimsIdentity;

        if (userIdentity?.IsAuthenticated is true)
        {
            var roles = account.AdditionalProperties[userIdentity.RoleClaimType] as JsonElement?;

            if (roles?.ValueKind == JsonValueKind.Array)
            {
                userIdentity.TryRemoveClaim(userIdentity.Claims.FirstOrDefault(c => c.Type == userIdentity.RoleClaimType));

                foreach (var element in roles.Value.EnumerateArray())
                {
                    userIdentity.AddClaim(new Claim(userIdentity.RoleClaimType, element.GetString()));
                }
            }
        }

        return userAccount;
    }
}