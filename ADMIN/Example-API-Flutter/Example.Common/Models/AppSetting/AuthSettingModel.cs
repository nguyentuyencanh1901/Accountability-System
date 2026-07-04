using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Models.AppSetting
{
    public class AuthSettingModel
    {
        public bool IsCheckToken { get; set; }
        public Guid UserId { get; set; }
        public string ApiVerifyToken { get; set; }
        public Guid ProductId { get; set; }
    }
}
