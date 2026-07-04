using Example.Common.Models;
using Example.UserService.WebApp.Models.Profile;
using Example.UserService.WebApp.Repository.IRepository;
using Example.UserService.WebApp.Services.IServices;

namespace Example.UserService.WebApp.Services
{
    /// <summary>
    /// Dịch vụ hồ sơ thí sinh: đọc thông tin user, dựng form sửa, cập nhật profile và đổi mật khẩu.
    /// Phân tách nguồn dữ liệu — đọc qua AppUserRepository, ghi qua AuthenticateRepository (self-service API).
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly IAuthenticateRepository _authenticateRepository;

        // Đọc hồ sơ qua AppUser; cập nhật/đổi mật khẩu qua Authenticate (self-service API).
        public ProfileService(IAppUserRepository appUserRepository, IAuthenticateRepository authenticateRepository)
        {
            _appUserRepository = appUserRepository;
            _authenticateRepository = authenticateRepository;
        }

        /// <summary>Tải hồ sơ thí sinh theo userId từ claims/JWT.</summary>
        public async Task<ProfileViewModel> GetProfileAsync(long userId)
        {
            // Quy tắc nghiệp vụ: userId không hợp lệ thì không gọi API
            if (userId <= 0)
                return new ProfileViewModel { ErrorMessage = "Không xác định được người dùng." };

            var result = await _appUserRepository.GetByIdAsync(userId);
            if (!result.Success || result.Data == null)
            {
                return new ProfileViewModel
                {
                    ErrorMessage = result.Message ?? "Không thể tải thông tin cá nhân."
                };
            }

            return new ProfileViewModel { User = result.Data };
        }

        /// <summary>Dựng form chỉnh sửa từ dữ liệu hồ sơ hiện tại (không cho sửa username).</summary>
        public async Task<EditProfileViewModel> BuildEditFormAsync(long userId)
        {
            var profile = await GetProfileAsync(userId);
            if (profile.User == null)
            {
                return new EditProfileViewModel { ErrorMessage = profile.ErrorMessage };
            }

            var user = profile.User;
            // Chỉ ánh xạ các trường được phép chỉnh sửa lên form
            return new EditProfileViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone
            };
        }

        /// <summary>
        /// Cập nhật hồ sơ qua API self-service (JWT xác định user, không cần truyền userId lên API).
        /// Trả về FullName/Email đã trim để Controller cập nhật claims/session.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage, string? FullName, string? Email)> UpdateProfileAsync(long userId, EditProfileViewModel model)
        {
            if (userId <= 0)
                return (false, "Không xác định được người dùng.", null, null);

            var result = await _authenticateRepository.UpdateProfileAsync(new UpdateProfileModel
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone
            });

            if (!result.Success)
            {
                return (false, result.Message ?? "Cập nhật thông tin thất bại.", null, null);
            }

            return (true, null, model.FullName.Trim(), model.Email.Trim());
        }

        /// <summary>Đổi mật khẩu: API kiểm tra mật khẩu hiện tại và xác nhận mật khẩu mới.</summary>
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(long userId, ChangePasswordViewModel model)
        {
            if (userId <= 0)
                return (false, "Không xác định được người dùng.");

            var result = await _authenticateRepository.ChangePasswordAsync(new ChangePasswordModel
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            });

            if (!result.Success)
            {
                return (false, result.Message ?? "Đổi mật khẩu thất bại.");
            }

            return (true, null);
        }
    }
}
