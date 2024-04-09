using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlvTime.Business.Utils;

public class DateAlvTime
{
    public IDateAlvTimeProvider Provider = new DateAlvTimeProvider();

    public DateTime Now => Provider.Now;
}

public interface IDateAlvTimeProvider
{
    DateTime Now { get; }
}

public class DateAlvTimeProvider : IDateAlvTimeProvider
{
    public DateTime Now => DateTime.Now;
}