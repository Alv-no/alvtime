using System.Threading.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.Interfaces
{
    public interface IUserContext
    {
        Task<User> GetCurrentUser();
    }
}
