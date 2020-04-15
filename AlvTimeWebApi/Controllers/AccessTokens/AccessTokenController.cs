using AlvTimeWebApi.Dto;
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

        public AccessTokenController(AlvTime_dbContext database)
        {
            _database = database;
        }

        [HttpPost("AccessToken")]
        [Authorize]
        public ActionResult<string> CreateLifetimeToken([FromBody] AccessTokenRequestDto request)
        {
            var user = RetrieveUser();

            var uuid = Guid.NewGuid().ToString();

            return CreateToken(user, uuid, request.FriendlyName);
        }

        [HttpDelete("AccessToken")]
        [Authorize]
        public ActionResult<int> DeleteAccessToken([FromBody] DeleteAccessTokenDto token)
        {
            var user = RetrieveUser();

            return DeleteToken(user, token.TokenId);
        }

        [HttpGet("ActiveAccessTokens")]
        [Authorize]
        public ActionResult<IEnumerable<AccessTokenResponseDto>> FetchFriendlyNames()
        {
            var user = RetrieveUser();

            var tokens = _database.AccessTokens
                .Where(x => x.UserId == user.Id && x.ExpiryDate >= DateTime.UtcNow)
                .Select(x => new AccessTokenResponseDto
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

            return Ok(uuid + " expires " + accessToken.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        private ActionResult<int> DeleteToken(User user, int tokenId)
        {
            var accessToken = _database.AccessTokens
                            .FirstOrDefault(x => x.Id == tokenId && x.UserId == user.Id);

            accessToken.ExpiryDate = DateTime.UtcNow;
            _database.SaveChanges();

            return Ok(accessToken.Value);
        }

        private User RetrieveUser()
        {
            var username = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var email = User.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var alvUser = _database.User.FirstOrDefault(x => x.Email.Equals(email));

            return alvUser;
        }
    }
}