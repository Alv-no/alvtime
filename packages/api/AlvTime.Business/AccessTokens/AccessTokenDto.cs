using System;

namespace AlvTime.Business.AccessTokens
{
    public record AccessTokenDto (int Id, string Token, DateTime ExpiryDate, string FriendlyName);
}
