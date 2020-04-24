using System;

namespace AlvTimeWebApi.Dto
{
    public class AccessTokenResponseDto
    {
        public string Token { get; set; }
        public string ExpiryDate { get; set; }
    }
}
