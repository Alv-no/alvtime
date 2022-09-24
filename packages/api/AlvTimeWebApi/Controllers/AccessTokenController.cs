using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Utils;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class AccessTokenController : Controller
    {
        private readonly AccessTokenService _tokenService;

        public AccessTokenController(AccessTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("AccessToken")]
        [Authorize]
        public ActionResult<AccessTokenCreatedResponse> CreateLifetimeToken(
            [FromBody] AccessTokenCreateRequest createRequest)
        {
            var accessToken = _tokenService.CreateLifeTimeToken(createRequest.FriendlyName);

            return Ok(new AccessTokenCreatedResponse(accessToken.Token, accessToken.ExpiryDate.ToDateOnly()));
        }

        [HttpDelete("AccessToken")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenFriendlyNameResponse>> DeleteAccessToken(
            [FromBody] IEnumerable<AccessTokenDeleteRequest> tokenIds)
        {
            var accessTokens = _tokenService.DeleteActiveTokens(tokenIds.Select(tokenId => tokenId.TokenId));

            return Ok(accessTokens.Select(token =>
                new AccessTokenFriendlyNameResponse(token.Id, token.FriendlyName, token.ExpiryDate.ToDateOnly())));
        }

        [HttpGet("ActiveAccessTokens")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenFriendlyNameResponse>> FetchFriendlyNames()
        {
            var accessTokens = _tokenService.GetActiveTokens();

            return Ok(accessTokens.Select(token =>
                new AccessTokenFriendlyNameResponse(token.Id, token.FriendlyName, token.ExpiryDate.ToDateOnly())));
        }
    }
}