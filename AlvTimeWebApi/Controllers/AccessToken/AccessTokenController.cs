using AlvTime.Business.AccessToken;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeApi.Controllers.AccessToken
{
    [Route("api/user")]
    [ApiController]
    public class AccessTokenController : Controller
    {
        private readonly IAccessTokenStorage _storage;
        private RetrieveUsers _userRetriever;

        public AccessTokenController(RetrieveUsers userRetriever, IAccessTokenStorage storage)
        {
            _storage = storage;
            _userRetriever = userRetriever;
        }

        [HttpPost("AccessToken")]
        [Authorize]
        public ActionResult<AccessTokenResponseDto> CreateLifetimeToken([FromBody] AccessTokenRequestDto request)
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage.CreateLifetimeToken(request.FriendlyName, user.Id));
        }

        [HttpDelete("AccessToken")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenFriendlyNameResponseDto>> DeleteAccessToken([FromBody] IEnumerable<DeleteAccessTokenDto> tokenIds)
        {
            var user = _userRetriever.RetrieveUser();

            var response = new List<AccessTokenFriendlyNameResponseDto>();

            foreach (var token in tokenIds)
            {
                response.Add(_storage.DeleteActiveTokens(token.TokenId, user.Id));
            }

            return Ok(response);
        }

        [HttpGet("ActiveAccessTokens")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenFriendlyNameResponseDto>> FetchFriendlyNames()
        {
            var user = _userRetriever.RetrieveUser();

            return Ok(_storage.GetActiveTokens(user.Id));
        }
    }
}
