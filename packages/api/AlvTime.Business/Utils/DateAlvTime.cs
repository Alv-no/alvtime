using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlvTime.Business.Utils
{
    public class DateAlvTime
    {
        public IDateAlvTimeProvider _provider = new DateAlvTimeProvider();
        public DateTime Now { get { 
                return _provider.Now; 
            } }

    }

    public interface IDateAlvTimeProvider
    {
        DateTime Now { get; }
    }

    public class DateAlvTimeProvider: IDateAlvTimeProvider
    {
        public DateTime Now { get { return DateTime.Now; } }

    }
}
