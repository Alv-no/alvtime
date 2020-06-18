using AlvTime.Business.Users;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.Users.UserStorage
{
    public static class UserQueryableExtensions
    {
        public static IQueryable<User> Filter(this IQueryable<User> query, UserQuerySearch criterias)
        {
            if (criterias.Email != null)
            {
                query = query.Where(user => user.Email == criterias.Email);
            }

            if (criterias.Name != null)
            {
                query = query.Where(user => user.Name == criterias.Name);
            }

            return query;
        }
    }
}
