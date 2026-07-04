using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Const
{
    public class ClaimConst
    {
        public const string UserId = "https://fpt.com/identity/claims/userid"; //ClaimTypes.NameIdentifier; 
        public const string CustomerId = "https://fpt.com/identity/claims/customerid";
        public const string CustomerCode = "https://fpt.com/identity/claims/customercode";
        public const string UserType = "https://fpt.com/identity/claims/usertype";
        //public const string UserName = "https://fpt.com/identity/claims/username"; // ClaimTypes.Name;
        public const string FullName = "https://fpt.com/identity/claims/fullname";
        public const string Email = "https://fpt.com/identity/claims/email";
        public const string Phone = "https://fpt.com/identity/claims/phone"; //ClaimTypes.MobilePhone;
        public const string Avatar = "https://fpt.com/identity/claims/avatar";
        //public const string Role = "https://fpt.com/identity/claims/role"; //ClaimTypes.Role;
        public const string Expired = "https://fpt.com/identity/claims/expired"; // ClaimTypes.Expired;
        //public const string Product = "https://fpt.com/identity/claims/permission";
        public const string DeviceId = "https://fpt.com/identity/claims/deviceid";
        //public const string ChecksumKey = "https://fpt.com/identity/claims/checksumkey";
        public const string GroupRoles = "https://fpt.com/identity/claims/grouproles";
        public const string Areas = "https://fpt.com/identity/claims/areas";
        public const string Departments = "https://fpt.com/identity/claims/departments";
    }
}
