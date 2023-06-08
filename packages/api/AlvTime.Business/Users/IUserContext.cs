using System.Threading.Tasks;

namespace AlvTime.Business.Users
{
    public interface IUserContext
    {
        Task<User> GetCurrentUser();
    }
}
