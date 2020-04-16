using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.Users.UserStorage
{
    public static class UseryQueryableExtensions
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
