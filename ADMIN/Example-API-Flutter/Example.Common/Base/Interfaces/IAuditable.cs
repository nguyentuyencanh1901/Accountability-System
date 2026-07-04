using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Base.Interfaces
{
    public interface IAuditable : IUserTracking, IDateTracking
    {
    }
}
