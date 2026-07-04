using System.ComponentModel.DataAnnotations;

namespace Example.Common.Models
{
    public class LoginModel
    {
        //[Description("Username")]
        /// <summary>
        /// Tên đang nhập
        /// </summary>
        /// <example>example</example>
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        /// <summary>
        /// Mật khẩu
        /// </summary>
        /// <example></example>
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

  /*      /// <summary>
        /// Code (Mã khách hàng)
        /// </summary>
        /// <example></example>
        public string? Code { get; set; }*/
    }

    public class LoginAppModel : LoginModel
    {
        public string DeviceId { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public string DeviceTokenFireBase { get; set; } = "";
    }
}
