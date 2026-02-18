using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Utils;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class AccessTokenController : Controller
    {
        private readonly AccessTokenService _tokenService;

        public AccessTokenController(AccessTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("AccessToken")]
        public async Task<ActionResult<AccessTokenCreatedResponse>> CreateLifetimeToken(
            [FromBody] AccessTokenCreateRequest createRequest)
        {
            var accessToken = await _tokenService.CreateLifeTimeToken(createRequest.FriendlyName);

            return Ok(new AccessTokenCreatedResponse(accessToken.Token, accessToken.ExpiryDate.ToDateOnly()));
        }

        [HttpDelete("AccessToken/{tokenId}")]
        public async Task<ActionResult> DeleteAccessToken(int tokenId)
        {
            await _tokenService.DeleteToken(tokenId);

            return NoContent();
        }

        [HttpGet("ActiveAccessTokens")]
        public async Task<ActionResult<IEnumerable<AccessTokenFriendlyNameResponse>>> FetchFriendlyNames()
        {
            var accessTokens = await _tokenService.GetActiveTokens();

            return Ok(accessTokens.Select(token =>
                new AccessTokenFriendlyNameResponse(token.Id, token.FriendlyName, token.ExpiryDate.ToDateOnly())));
        }
    }
}