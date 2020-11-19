using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.Economy
{
    public interface IEconomyStorage
    {
        IEnumerable<DataDumpDto> GetEconomyInfo();
    }
}
