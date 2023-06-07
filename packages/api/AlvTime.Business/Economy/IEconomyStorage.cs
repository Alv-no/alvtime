using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Economy;

public interface IEconomyStorage
{
    Task<IEnumerable<DataDumpDto>> GetEconomyInfo();
}