using System;

namespace AlvTimeWebApi.Dto
{
    public class AccessTokenResponseDto
    {
        public int Id { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
