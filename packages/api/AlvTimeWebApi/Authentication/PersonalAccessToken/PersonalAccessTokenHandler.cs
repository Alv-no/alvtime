using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AlvTime.Business.Models;
using AlvTime.Business.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlvTimeWebApi.Authentication.PersonalAccessToken
{
    public class PersonalAccessTokenHandler : AuthenticationHandler<PersonalAccessTokenOptions>
    {
        private readonly IUserRepository _repository;

        public PersonalAccessTokenHandler(
            IOptionsMonitor<PersonalAccessTokenOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserRepository repository,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _repository = repository;
        }

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

            var user = await _repository.GetUserFromToken(new Token {Value = token});
            if (user == null)
            {
                return AuthenticateResult.Fail("No user could be retrieved from the given token");
            }

            if (user.EndDate != null && user.EndDate < DateTime.Now)
            {
                return AuthenticateResult.Fail("The user has an end date in the past");
            }

            var claims = CreateClaims(user);

            return AuthenticateResult.Success(CreateTicket(claims));
        }

        private static AuthenticationTicket CreateTicket(IEnumerable<Claim> claims)
            => new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, "PersonalAccessToken")),
                "PersonalAccessTokenScheme");

        private static IEnumerable<Claim> CreateClaims(User user)
            => new[]
            {
                new Claim("preferred_username", user.Email),
                new Claim("name", user.Name)
            };
    }
}