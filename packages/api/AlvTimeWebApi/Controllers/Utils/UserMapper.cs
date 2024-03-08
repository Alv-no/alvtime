using System;
using AlvTime.Business.Users;

namespace AlvTimeWebApi.Controllers.Utils;

public static class UserMapper
{

    public static User MapUserDtoToBusinessUser(UserResponseDto dbUser)
    {
        return new User { Id = dbUser.Id, Email = dbUser.Email, Name = dbUser.Name, StartDate = DateTime.Parse(dbUser.StartDate) };
    } 
}