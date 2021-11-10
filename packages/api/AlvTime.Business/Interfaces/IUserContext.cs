using AlvTime.Business.Models;

namespace AlvTime.Business.Interfaces
{
    public interface IUserContext
    {
        public User GetCurrentUser();
    }
}
