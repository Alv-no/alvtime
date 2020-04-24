using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeApi.Controllers.AccessToken
{
    [Route("api/user")]
    [ApiController]
    public class AccessTokenController : Controller
    {
        private readonly AlvTime_dbContext _database;
        private RetrieveUsers _userRetriever;

        public AccessTokenController(AlvTime_dbContext database, RetrieveUsers userRetriever)
        {
            _database = database;
            _userRetriever = userRetriever;
        }

        [HttpPost("AccessToken")]
        [Authorize]
        public ActionResult<string> CreateLifetimeToken([FromBody] AccessTokenRequestDto request)
        {
            var user = _userRetriever.RetrieveUser();

            var uuid = Guid.NewGuid().ToString();

            return CreateToken(user, uuid, request.FriendlyName);
        }

        [HttpDelete("AccessToken")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenFriendlyNameResponseDto>> DeleteAccessToken([FromBody] IEnumerable<DeleteAccessTokenDto> tokenIds)
        {
            var user = _userRetriever.RetrieveUser();
            return DeleteTokens(user, tokenIds);
        }

        [HttpGet("ActiveAccessTokens")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenFriendlyNameResponseDto>> FetchFriendlyNames()
        {
            var user = _userRetriever.RetrieveUser();

            var tokens = _database.AccessTokens
                .Where(x => x.UserId == user.Id && x.ExpiryDate >= DateTime.UtcNow)
                .Select(x => new AccessTokenFriendlyNameResponseDto
                {
                    Id = x.Id,
                    FriendlyName = x.FriendlyName,
                    ExpiryDate = x.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                })
                .ToList();

            return Ok(tokens);
        }

        private ActionResult<string> CreateToken(User user, string uuid, string friendlyName)
        {
            var accessToken = new AccessTokens
            {
                UserId = user.Id,
                Value = uuid,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                FriendlyName = friendlyName
            };

            _database.AccessTokens.Add(accessToken);
            _database.SaveChanges();

            var token = new AccessTokenResponseDto
            {
                Token = uuid,
                ExpiryDate = accessToken.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            return Ok(token);
        }

        private ActionResult<IEnumerable<AccessTokenFriendlyNameResponseDto>> DeleteTokens(User user, IEnumerable<DeleteAccessTokenDto> tokens)
        {
            List<AccessTokenFriendlyNameResponseDto> deletedFriendlyNames = new List<AccessTokenFriendlyNameResponseDto>();

            foreach (var token in tokens)
            {
                var deletedFriendlyName = DeleteToken(user, token.TokenId);
                deletedFriendlyNames.Add(deletedFriendlyName);
            }

            return Ok(deletedFriendlyNames);
        }

        private AccessTokenFriendlyNameResponseDto DeleteToken(User user, int tokenId)
        {
            var accessToken = _database.AccessTokens
                            .FirstOrDefault(token => token.Id == tokenId && token.UserId == user.Id);

            accessToken.ExpiryDate = DateTime.UtcNow;
            _database.SaveChanges();

            var deletedFriendlyName = new AccessTokenFriendlyNameResponseDto
            {
                Id = accessToken.Id,
                FriendlyName = accessToken.FriendlyName,
                ExpiryDate = accessToken.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };

            return deletedFriendlyName;
        }
    }
}
