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

        if (userIdentity?.IsAuthenticated is not true) return userAccount;
        
        if (account.AdditionalProperties.TryGetValue(userIdentity.RoleClaimType, out var rolesObj) && rolesObj is JsonElement roles && roles.ValueKind == JsonValueKind.Array)
        {
            var roleClaim = userIdentity.Claims.FirstOrDefault(c => c.Type == userIdentity.RoleClaimType);
            if (roleClaim != null)
            {
                userIdentity.TryRemoveClaim(roleClaim);
            }

            foreach (var element in roles.EnumerateArray())
            {
                var role = element.GetString();
                if (!string.IsNullOrEmpty(role))
                {
                    userIdentity.AddClaim(new Claim(userIdentity.RoleClaimType, role));
                }
            }
        }

        return userAccount;
    }
}