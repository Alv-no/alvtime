using AlvTime.Business.AccessToken.PersonalAccessToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Authentication.PersonalAccessToken
{
    public class PersonalAccessTokenHandler : AuthenticationHandler<PersonalAccessTokenOptions>
    {
        private readonly IPersonalAccessTokenStorage _storage;

        public PersonalAccessTokenHandler(
            IOptionsMonitor<PersonalAccessTokenOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IPersonalAccessTokenStorage storage,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _storage = storage;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorization = Request.Headers["Authorization"]
                .ToString()
                .Split(' ');

            var token = authorization.Last();

            if (authorization.First() != "Bearer" || token == null)
            {
                return AuthenticateResult.Fail("Could not parse token");
            }

            var user = await _storage.GetUserFromToken(new Token { Value = token });
            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid token");
            }

            Claim[] claims = CreateClaims(user);
            return AuthenticateResult.Success(CreateTicket(claims));
        }

        private static AuthenticationTicket CreateTicket(Claim[] claims)
            => new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, "PersonalAccessToken")), "PersonalAccessTokenScheme");

        private static Claim[] CreateClaims(AlvTime.Business.AccessToken.User user)
            => new Claim[]
            {
                new Claim("preferred_username", user.Email),
                new Claim("name", user.Name)
            };
    }
}
