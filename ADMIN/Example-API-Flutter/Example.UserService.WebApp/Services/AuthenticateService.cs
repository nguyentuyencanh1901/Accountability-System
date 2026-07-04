using Example.API.Client.Models;
using Example.Common.Models;
using Example.UserService.WebApp.Const;
using Example.UserService.WebApp.Models;
using Example.UserService.WebApp.Repository.IRepository;
using Example.UserService.WebApp.Services.IServices;

namespace Example.UserService.WebApp.Services
{
    /// <summary>
    /// Dịch vụ xác thực thí sinh: ánh xạ ViewModel ↔ API model, kiểm tra kết quả đăng nhập/đăng ký,
    /// và cung cấp thông báo lỗi thân thiện cho giao diện WebApp.
    /// </summary>
    public class AuthenticateService : IAuthenticateService
    {
        private readonly IAuthenticateRepository _repository;

        // Tiêm facade xác thực; Service ánh xạ ViewModel và chuẩn hóa thông báo lỗi.
        public AuthenticateService(IAuthenticateRepository repository) => _repository = repository;

        /// <summary>
        /// Đăng nhập thí sinh: gọi API login-app, yêu cầu phải có token hợp lệ trong response.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage, LoginResponse? Data)> LoginAsync(LoginViewModel model)
        {
            // Chuyển đổi form đăng nhập sang DTO API trước khi gọi Repository (facade → client HTTP).
            var result = await _repository.LoginAppAsync(new LoginModel
            {
                Username = model.Username,
                Password = model.Password
            });

            // Quy tắc nghiệp vụ: đăng nhập chỉ thành công khi API báo Success, có Data và token không rỗng.
            if (!result.Success || result.Data == null || string.IsNullOrWhiteSpace(result.Data.Token))
            {
                return (false, string.IsNullOrWhiteSpace(result.Message)
                    ? "Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản."
                    : result.Message, null);
            }

            return (true, null, result.Data);
        }

        /// <summary>
        /// Đăng ký tài khoản mới: luôn gán loại user mặc định cho thí sinh, không gán role admin.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterViewModel model)
        {
            // Quy tắc nghiệp vụ: mọi đăng ký từ WebApp đều là thí sinh (UserType cố định).
            model.UserType = RegisterDefaults.UserType;

            var result = await _repository.RegisterAsync(new RegisterModel
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                UserType = model.UserType,
                Status = model.Status,
                // Quy tắc nghiệp vụ: thí sinh tự đăng ký không được gán role quản trị.
                Roles = new List<long>()
            });

            if (!result.Success)
            {
                return (false, string.IsNullOrWhiteSpace(result.Message)
                    ? "Đăng ký thất bại. Vui lòng thử lại."
                    : result.Message);
            }

            return (true, null);
        }

        /// <summary>Đăng xuất: xóa phiên JWT/cookie phía client qua Repository.</summary>
        public void Logout() => _repository.Logout();
    }
}
