using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlvTimeWebApi.Authentication.PersonalAccessToken;

public class PersonalAccessTokenHandler(
    IOptionsMonitor<PersonalAccessTokenOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserRepository repository)
    : AuthenticationHandler<PersonalAccessTokenOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization
            .ToString()
            .Split(' ');

        var token = authorization.LastOrDefault();

        if (authorization.First() != "Bearer" || token == null)
        {
            return AuthenticateResult.Fail(
                "Authorization header either doesn't start with \"Bearer\" or the token payload is null");
        }

        var user = await repository.GetUserFromToken(new Token {Value = token});
        if (user == null)
        {
            return AuthenticateResult.Fail("No user could be retrieved from the given token");
        }

        var claims = CreateClaims(user);

        return AuthenticateResult.Success(CreateTicket(claims));
    }

    private static AuthenticationTicket CreateTicket(IEnumerable<Claim> claims)
        => new(new ClaimsPrincipal(new ClaimsIdentity(claims, "PersonalAccessToken")),
            "PersonalAccessTokenScheme");

    private static IEnumerable<Claim> CreateClaims(User user)
        =>
        [
            new("preferred_username", user.Email),
            new("name", user.Name),
            new("http://schemas.microsoft.com/identity/claims/objectidentifier", user.Oid),
        ];
}