using System;

namespace AlvTimeWebApi.Dto
{
    public class AccessTokenFriendlyNameResponseDto
    {
        public int Id { get; set; }
        public string FriendlyName { get; set; }
        public string ExpiryDate { get; set; }
    }
}
