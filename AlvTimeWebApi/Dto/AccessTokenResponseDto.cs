using System;

namespace AlvTimeWebApi.Dto
{
    public class AccessTokenResponseDto
    {
        public int Id { get; set; }
        public string FriendlyName { get; set; }
        public string ExpiryDate { get; set; }
    }
}
