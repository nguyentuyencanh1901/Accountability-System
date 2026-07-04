using Example.API.Client.Models;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Const;
using Example.UserService.WebAdmin.Models;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho đăng nhập/đăng ký: map ViewModel ↔ API model, validate vai trò mặc định,
    /// chuẩn hóa thông báo lỗi cho UI.
    /// </summary>
    public class AuthenticateService : IAuthenticateService
    {
        // Repository ủy quyền gọi HTTP API đăng nhập/đăng ký
        private readonly IAuthenticateRepository _repository;
        // Repository vai trò — dùng gán vai trò mặc định khi đăng ký
        private readonly IRoleRepository _roleRepository;

        /// <summary>Khởi tạo service với repository xác thực và vai trò.</summary>
        public AuthenticateService(IAuthenticateRepository repository, IRoleRepository roleRepository)
        {
            _repository = repository;
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// Đăng nhập: map LoginViewModel → LoginModel, kiểm tra Success/Token trước khi trả về cho controller.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage, LoginResponse? Data)> LoginAsync(LoginViewModel model)
        {
            var result = await _repository.LoginAsync(new LoginModel
            {
                Username = model.Username,
                Password = model.Password
            });

            // Thất bại nếu API lỗi, không có Data, hoặc token rỗng
            if (!result.Success || result.Data == null || string.IsNullOrWhiteSpace(result.Data.Token))
            {
                return (false, string.IsNullOrWhiteSpace(result.Message)
                    ? "Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản."
                    : result.Message, null);
            }

            return (true, null, result.Data);
        }

        /// <summary>Chuẩn bị form đăng ký với giá trị mặc định (UserType, vai trò Admin).</summary>
        public async Task<RegisterViewModel> BuildRegisterViewModelAsync(RegisterViewModel model)
        {
            await ApplyRegisterDefaultsAsync(model);
            return model;
        }

        /// <summary>
        /// Gán UserType cố định và tự chọn vai trò Admin từ danh sách API (so khớp tên không phân biệt hoa thường).
        /// </summary>
        public async Task ApplyRegisterDefaultsAsync(RegisterViewModel model)
        {
            model.UserType = RegisterDefaults.UserType;

            var rolesResult = await _roleRepository.GetListAsync(new RoleSearchModel
            {
                PageIndex = 1,
                PageSize = 100
            });

            var adminRole = rolesResult.Success && rolesResult.Data != null
                ? rolesResult.Data.FirstOrDefault(r =>
                    string.Equals(r.Name, RegisterDefaults.RoleName, StringComparison.OrdinalIgnoreCase))
                : null;

            model.SelectedRoleIds = adminRole != null
                ? new List<long> { adminRole.Id }
                : new List<long>();
        }

        /// <summary>Validate đăng ký: phải tìm được vai trò mặc định trong hệ thống.</summary>
        public IEnumerable<(string Key, string Message)> ValidateRegister(RegisterViewModel model)
        {
            if (model.SelectedRoleIds == null || model.SelectedRoleIds.Count == 0)
                yield return (string.Empty, $"Không tìm thấy vai trò '{RegisterDefaults.RoleName}' trong hệ thống.");
        }

        /// <summary>Map RegisterViewModel → RegisterModel và gọi API đăng ký.</summary>
        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterViewModel model)
        {
            var result = await _repository.RegisterAsync(new RegisterModel
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                UserType = model.UserType,
                Roles = model.SelectedRoleIds ?? new List<long>()
            });

            if (!result.Success)
            {
                return (false, string.IsNullOrWhiteSpace(result.Message)
                    ? "Đăng ký thất bại. Vui lòng thử lại."
                    : result.Message);
            }

            return (true, null);
        }

        /// <summary>Đổi mật khẩu tài khoản đang đăng nhập.</summary>
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var result = await _repository.ChangePasswordAsync(new ChangePasswordModel
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            });

            if (!result.Success)
            {
                return (false, string.IsNullOrWhiteSpace(result.Message)
                    ? "Đổi mật khẩu thất bại."
                    : result.Message);
            }

            return (true, null);
        }

        /// <summary>Ủy quyền đăng xuất (xóa token phía client).</summary>
        public void Logout() => _repository.Logout();
    }
}
