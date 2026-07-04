using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Base.Interfaces
{
    public interface IDateTracking
    {
        // DateTimeOffset is better for Date & Time convert from UTC to Local
        DateTimeOffset CreatedDate { get; set; }
        DateTimeOffset? LastModifiedDate { get; set; }
    }
}
