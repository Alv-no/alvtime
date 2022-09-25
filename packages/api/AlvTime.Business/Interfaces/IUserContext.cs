using System.Threading.Tasks;
using AlvTime.Business.Models;

namespace AlvTime.Business.Interfaces
{
    public interface IUserContext
    {
        Task<User> GetCurrentUser();
    }
}
