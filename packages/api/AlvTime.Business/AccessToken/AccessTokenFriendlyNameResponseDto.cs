using System;

namespace AlvTime.Business.AccessToken
{
    public class AccessTokenFriendlyNameResponseDto
    {
        public int Id { get; set; }
        public string FriendlyName { get; set; }
        public string ExpiryDate { get; set; }
    }
}
