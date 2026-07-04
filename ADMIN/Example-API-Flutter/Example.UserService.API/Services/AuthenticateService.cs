using Example.Common.Const;
using Example.Common.Cache;
using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Entities;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Services.IServices;
using FirebaseAdmin.Auth.Hash;
using Mapster;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Example.UserService.API.Services
{
    /// <summary>
    /// Xác thực: đăng ký, đăng nhập (WebAdmin/WebApp), JWT, cập nhật hồ sơ và đổi mật khẩu.
    /// Register/UpdateProfile invalidate cache <c>AppUser_GetListPaging</c>.
    /// </summary>
    public class AuthenticateService : IAuthenticateService
    {
        // Ghi log lỗi
        private readonly ILogger<AuthenticateService> _logger;
        // Truy cập bảng AppUser
        private readonly IAppUserRepository _appUserRepository;
        // Đọc cấu hình JWT từ appsettings
        private readonly IConfiguration _configuration;
        // Gán role khi đăng ký Manager
        private readonly IUserRoleRepository _userRoleRepository;
        // Invalidate cache danh sách user sau Register/UpdateProfile
        private readonly IRedisCache _redisCache;

        public AuthenticateService(ILogger<AuthenticateService> logger, IAppUserRepository appUserRepository, IConfiguration configuration, IUserRoleRepository userRoleRepository, IRedisCache redisCache)
        {
            _appUserRepository = appUserRepository;
            _logger = logger;
            _configuration = configuration;
            _userRoleRepository = userRoleRepository;
            _redisCache = redisCache;
        }

        /// <summary>Đăng ký tài khoản mới; hash mật khẩu; gán role nếu Manager.</summary>
        public async Task<ResponseData<object>> Register(RegisterModel model)
        {
            try
            {
                // Kiểm tra đủ trường bắt buộc
                if (string.IsNullOrWhiteSpace(model.Username) ||
                    string.IsNullOrWhiteSpace(model.Password) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.FullName))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                // Username / Email / FullName không trùng user khác
                if (await _appUserRepository.CheckUserNameExists(model.Username, 0))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeDoesNotExist);
                }
                if (await _appUserRepository.CheckEmailExists(model.Email, 0))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeDoesNotExist);
                }
                if (await _appUserRepository.CheckNameExists(model.FullName, 0))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeDoesNotExist);
                }
                // UserType không hợp lệ → mặc định là thí sinh
                if (!IsValidUserType(model.UserType))
                {
                    model.UserType = (int)UserTypeEnum.TestTaker;
                }

                if (model.UserType == (int)UserTypeEnum.Manager)
                {
                    // Loại bỏ Role trùng trước khi gán
                    model.Roles = model.Roles?.Distinct().ToList();
                    // Manager bắt buộc có ít nhất một Role để gán permission
                    if (model.Roles == null || model.Roles.Count == 0)
                    {
                        return new ResponseData<object>("Người quản lý bắt buộc phải chọn Role");
                    }
                }

                var user = model.Adapt<AppUser>();
                // Hash mật khẩu trước khi lưu — không lưu plain text
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                await _appUserRepository.CreateAsync(user);
                await _appUserRepository.SaveChangesAsync();

                // Manager: tạo bản ghi UserRole cho từng Role đã chọn
                if (model.UserType == (int)UserTypeEnum.Manager && model.Roles != null && model.Roles.Count > 0)
                {
                    var Role = model.Roles.Select(d => new UserRole
                    {
                        UserId = user.Id,
                        RoleId = d
                    });

                    await _userRoleRepository.CreateListAsync(Role);
                }

                await _appUserRepository.SaveChangesAsync();
                await _appUserRepository.EndTransactionAsync();
                // Danh sách user admin có cache — xóa sau khi thêm user mới
                await _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", "AppUser_GetListPaging"));
                return new ResponseData<object>(true, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Đăng nhập WebAdmin — chỉ cho phép UserType Manager.</summary>
        public async Task<ResponseData<object>> Login(LoginModel model)
            => await LoginInternal(model, UserTypeEnum.Manager, "Tài khoản không có quyền đăng nhập hệ thống quản trị");

        /// <summary>Đăng nhập WebApp thí sinh — chỉ cho phép UserType TestTaker.</summary>
        public async Task<ResponseData<object>> LoginApp(LoginModel model)
            => await LoginInternal(model, UserTypeEnum.TestTaker, "Tài khoản không có quyền đăng nhập hệ thống thi");

        /// <summary>Đăng nhập theo UserType (Manager → WebAdmin, TestTaker → WebApp).</summary>
        private async Task<ResponseData<object>> LoginInternal(LoginModel model, UserTypeEnum requiredUserType, string wrongTypeMessage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Username) ||
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var user = await _appUserRepository.GetByUsernameAsync(model.Username);
                if (user == null)
                {
                    return new ResponseData<object>("Username hoặc password không đúng");
                }
                // Không tiết lộ user tồn tại hay không — cùng một message
                bool isValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                if (!isValid)
                {
                    return new ResponseData<object>("Username hoặc password không đúng");
                }
                // WebAdmin chỉ Manager; WebApp chỉ TestTaker
                if (user.UserType != (int)requiredUserType)
                {
                    return new ResponseData<object>(wrongTypeMessage);
                }
                var token = await GenerateToken(user);
                return new ResponseData<object>(true, new
                {
                    Token = token,
                    User = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Email,
                        user.UserType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }


        /// <summary>Tạo JWT kèm permission claim; SUPER_ADMIN bypass kiểm tra quyền ở AuthorizeAttribute.</summary>
        private async Task<string> GenerateToken(AppUser user)
        {
            // Đọc cấu hình JWT từ appsettings
            var jwt = _configuration.GetSection("JWT");

            var secret = jwt["Secret"];
            var issuer = jwt["Issuer"];
            var audience = jwt["Audience"];
            var expireMinutes = int.Parse(jwt["ExpireMinutes"] ?? "30");

            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("JWT Secret đang NULL");
            }

            // business rules: load permission từ role của user (manager)
            var permissions = await _appUserRepository.GetUserPermissions(user.Id);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(ClaimConst.UserType, user.UserType.ToString())
    };

            // Mỗi permission là một claim — AuthorizeAttribute kiểm tra khi gọi API
            foreach (var p in permissions)
            {
                claims.Add(new Claim("permission", p));
            }

            // Ký token bằng HMAC-SHA256 với secret đối xứng
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool IsValidUserType(int userType)
        {
            return userType == (int)UserTypeEnum.TestTaker
                || userType == (int)UserTypeEnum.Manager;
        }

        /// <summary>Thí sinh tự sửa FullName/Email/Phone; không đổi Username hay quyền.</summary>
        public async Task<ResponseData<object>> UpdateProfile(long userId, UpdateProfileModel model)
        {
            try
            {
                if (userId <= 0 ||
                    string.IsNullOrWhiteSpace(model.FullName) ||
                    string.IsNullOrWhiteSpace(model.Email))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var user = await _appUserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                if (user.Status != (int)StatusEnum.Active)
                {
                    return new ResponseData<object>("Tài khoản không hoạt động.");
                }

                // Unique FullName / Email / Phone (trừ chính user đang sửa)
                if (await _appUserRepository.CheckNameExists(model.FullName.Trim(), userId))
                {
                    return new ResponseData<object>("Họ tên đã được sử dụng.");
                }

                if (await _appUserRepository.CheckEmailExists(model.Email.Trim(), userId))
                {
                    return new ResponseData<object>("Email đã được sử dụng.");
                }

                if (await _appUserRepository.CheckPhoneExists(model.Phone, userId))
                {
                    return new ResponseData<object>("Số điện thoại đã được sử dụng.");
                }

                user.FullName = model.FullName.Trim();
                user.Email = model.Email.Trim();
                user.Phone = model.Phone?.Trim() ?? string.Empty;
                await _appUserRepository.UpdateAsync(user);
                await _appUserRepository.SaveChangesAsync();
                // Họ tên hiển thị trên danh sách admin có thể đổi → invalidate cache
                await _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", "AppUser_GetListPaging"));

                return new ResponseData<object>(true, new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.Phone,
                    user.UserType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Đổi mật khẩu: xác minh mật khẩu cũ bằng BCrypt.</summary>
        public async Task<ResponseData<object>> ChangePassword(long userId, ChangePasswordModel model)
        {
            try
            {
                if (userId <= 0 ||
                    string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(model.NewPassword) ||
                    string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    return new ResponseData<object>("Mật khẩu xác nhận không khớp.");
                }

                if (model.NewPassword.Length < 6)
                {
                    return new ResponseData<object>("Mật khẩu mới tối thiểu 6 ký tự.");
                }

                var user = await _appUserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    return new ResponseData<object>("Mật khẩu hiện tại không đúng.");
                }

                // Hash mới trước khi lưu — không invalidate cache (không ảnh hưởng list)
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _appUserRepository.UpdateAsync(user);
                await _appUserRepository.SaveChangesAsync();

                return new ResponseData<object>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

    }
}

