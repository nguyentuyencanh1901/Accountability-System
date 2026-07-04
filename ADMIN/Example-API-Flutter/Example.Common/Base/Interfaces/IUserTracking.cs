using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Base.Interfaces
{
    public interface IUserTracking
    {        
        string? CreatedBy { get; set; }        
        string? LastModifiedBy { get; set; }
    }
}
