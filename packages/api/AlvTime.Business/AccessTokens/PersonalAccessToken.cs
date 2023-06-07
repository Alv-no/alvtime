using System;
using AlvTime.Business.Users;

namespace AlvTime.Business.AccessTokens
{
    public class PersonalAccessToken
    {
        public User User { get; set; }
        public string Value { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string FriendlyName { get; set; }
    }
}