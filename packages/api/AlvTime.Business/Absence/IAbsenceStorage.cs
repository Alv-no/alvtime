using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Absence;

public interface IAbsenceStorage
{
    Task<IEnumerable<CustomVacationOverrideOverview>> GetCustomVacationEarned(int userId);
}
