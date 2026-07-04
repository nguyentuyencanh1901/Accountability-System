using Example.Common.Cache;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Example.UserService.API.Repository;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Services.IServices;
using Mapster;

namespace Example.UserService.API.Services
{
    /// <summary>
    /// CRUD người dùng (admin): thí sinh và quản lý.
    /// Cache Redis prefix <c>AppUser_GetListPaging</c>; invalidate khi CUD.
    /// </summary>
    public class AppUserService : IAppUserService
    {
        // Prefix Redis danh sách user
        private const string ListCacheKey = "AppUser_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<AppUserService> _logger;
        // CRUD AppUser
        private readonly IAppUserRepository _appUserRepository;
        // Đồng bộ UserRole khi CUD manager
        private readonly IUserRoleRepository _userRoleRepository;
        // Chặn xóa user có lịch sử thi
        private readonly IExamSessionRepository _examSessionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public AppUserService(
            ILogger<AppUserService> logger,
            IAppUserRepository appUserRepository,
            IUserRoleRepository userRoleRepository,
            IExamSessionRepository examSessionRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _appUserRepository = appUserRepository;
            _userRoleRepository = userRoleRepository;
            _examSessionRepository = examSessionRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa user; chặn SUPER_ADMIN và user có lịch sử thi.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _appUserRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                if (await IsSuperAdminUserAsync(id))
                {
                    // Bảo vệ tài khoản SUPER_ADMIN — không cho xóa
                    return new ResponseData<object>("Không được xóa tài khoản có quyền SUPER_ADMIN");
                }

                if (await _examSessionRepository.CountByUserId(id) > 0)
                {
                    // Có lịch sử thi → giữ dữ liệu audit
                    return new ResponseData<object>("Không thể xóa người dùng đã có lịch sử thi.");
                }

                // Transaction: xóa UserRole rồi xóa user
                await _appUserRepository.BeginTransactionAsync();
                var exist = await _userRoleRepository.GetById(id);
                if (exist.Count > 0)
                {
                    await _userRoleRepository.DeleteListAsync(exist);
                }
                await _appUserRepository.DeleteAsync(entity);
                await _appUserRepository.SaveChangesAsync();
                await _appUserRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, id);
            }
            catch (Exception ex)
            {
                await _appUserRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết user kèm danh sách RoleId (nếu Manager) và cờ SUPER_ADMIN.</summary>
        public async Task<ResponseData<AppUserModel>> GetById(long id)
        {
            try
            {

                if (id <= 0)
                {
                    return new ResponseData<AppUserModel>(ErrorCodeAPI.InvalidInput);
                }
                var entity = await _appUserRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<AppUserModel>(ErrorCodeAPI.NotFound);
                }
                var model = entity.Adapt<AppUserModel>();

                if (entity.UserType == (int)UserTypeEnum.Manager)
                {
                    // Manager: load danh sách RoleId đã gán
                    var users = await _userRoleRepository
                        .GetByListId(new List<long> { id });

                    model.Roles = users.Select(x => x.RoleId).ToList();
                }
                else
                {
                    model.Roles = new List<long>();
                }

                model.IsSuperAdmin = await IsSuperAdminUserAsync(id);
                return new ResponseData<AppUserModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<AppUserModel>(ex.Message);
            }
        }

        /// <summary>Danh sách user phân trang; cache Redis; gắn cờ SUPER_ADMIN mỗi dòng.</summary>
        public async Task<ResponseData<IEnumerable<AppUserModel>>> GetListPaging(AppUserSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.UserType, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _appUserRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _appUserRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var list = listPaging.ToList();
                            // Gắn cờ SUPER_ADMIN cho từng dòng (dùng hiển thị UI)
                            foreach (var item in list)
                            {
                                item.IsSuperAdmin = await IsSuperAdminUserAsync(item.Id);
                            }

                            var pagedList = new PagedList<AppUserModel>(list, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<AppUserModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<AppUserModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<AppUserModel>>(ex.Message);
            }
        }

        /// <summary>Tạo user mới; hash mật khẩu; gán role nếu Manager (transaction).</summary>
        public async Task<ResponseData<object>> Insert(AppUserSaveModel model)
        {
            // transaction: user + userRole
            await _appUserRepository.BeginTransactionAsync();
            try
            {
                // Validate unique Username / FullName / Email và UserType
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (await _appUserRepository.CheckNameExists(model.FullName, model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                if (await _appUserRepository.CheckEmailExists(model.Email, model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                if (await _appUserRepository.CheckUserNameExists(model.Username, model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                if (!IsValidUserType(model.UserType))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var roleValidation = ValidateRolesForUserType(model.UserType, model.Roles);
                if (roleValidation != null)
                {
                    return roleValidation;
                }

                var data = model.Adapt<AppUser>();
                // PasswordHash từ form admin — hash BCrypt trước khi lưu
                data.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                await _appUserRepository.CreateAsync(data);
                await _appUserRepository.SaveChangesAsync();

                await AssignUserRoles(data.Id, model.UserType, model.Roles);

                await _appUserRepository.SaveChangesAsync();
                await _appUserRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                await _appUserRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật user; đồng bộ role; hash mật khẩu mới (transaction).</summary>
        public async Task<ResponseData<object>> Update(AppUserSaveModel model)
        {
            // Kiểm tra SUPER_ADMIN trước transaction — Dapper không dùng chung transaction EF được
            var isSuperAdmin = await IsSuperAdminUserAsync(model.Id);

            // transaction: user + đồng bộ userRole
            await _appUserRepository.BeginTransactionAsync();
            try
            {
                var entity = await _appUserRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                // SUPER_ADMIN: cho sửa họ tên, email, SĐT, mật khẩu — giữ nguyên username/loại/trạng thái/vai trò
                if (isSuperAdmin)
                {
                    if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
                    {
                        await _appUserRepository.RollbackTransactionAsync();
                        return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                    }

                    if (await _appUserRepository.CheckEmailExists(model.Email, model.Id))
                    {
                        await _appUserRepository.RollbackTransactionAsync();
                        return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                    }

                    entity.FullName = model.FullName.Trim();
                    entity.Email = model.Email.Trim();
                    entity.Phone = model.Phone?.Trim() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(model.PasswordHash) && model.PasswordHash != "********")
                    {
                        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                    }

                    await _appUserRepository.UpdateAsync(entity);
                    await _appUserRepository.SaveChangesAsync();
                    await _appUserRepository.EndTransactionAsync();
                    await InvalidateListCacheAsync();
                    return new ResponseData<object>(true, entity);
                }

                // Validate unique Username / FullName / Email và UserType
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                if (await _appUserRepository.CheckNameExists(model.FullName, model.Id))
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                if (await _appUserRepository.CheckEmailExists(model.Email, model.Id))
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                if (await _appUserRepository.CheckUserNameExists(model.Username, model.Id))
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                if (!IsValidUserType(model.UserType))
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var roleValidation = ValidateRolesForUserType(model.UserType, model.Roles);
                if (roleValidation != null)
                {
                    await _appUserRepository.RollbackTransactionAsync();
                    return roleValidation;
                }

                var updateModel = model.Adapt(entity);
                if (!string.IsNullOrWhiteSpace(model.PasswordHash) && model.PasswordHash != "********")
                {
                    updateModel.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                }
                else
                {
                    updateModel.PasswordHash = entity.PasswordHash;
                }
                await _appUserRepository.UpdateAsync(updateModel);

                #region UserRole
                // Đồng bộ role: xóa hết cũ, gán lại theo form (chỉ Manager)
                var existingUserRole = await _userRoleRepository.GetById(entity.Id);
                if (existingUserRole.Count > 0)
                {
                    await _userRoleRepository.DeleteListAsync(existingUserRole);
                }

                await AssignUserRoles(updateModel.Id, model.UserType, model.Roles);
                #endregion

                await _appUserRepository.SaveChangesAsync();
                await _appUserRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {

                await _appUserRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Admin reset mật khẩu user; mặc định 123456 nếu không truyền NewPassword.</summary>
        public async Task<ResponseData<object>> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                if (model.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                if (await IsSuperAdminUserAsync(model.Id))
                {
                    return new ResponseData<object>("Không thể reset mật khẩu tài khoản SUPER_ADMIN");
                }

                var entity = await _appUserRepository.GetByIdAsync(model.Id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var newPassword = model.NewPassword?.Trim();
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    return new ResponseData<object>("Vui lòng nhập mật khẩu mới");
                }
                if (newPassword.Length < 6)
                {
                    return new ResponseData<object>("Mật khẩu mới tối thiểu 6 ký tự");
                }

                entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _appUserRepository.UpdateAsync(entity);
                await _appUserRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private static bool IsValidUserType(int userType)
        {
            return userType == (int)UserTypeEnum.TestTaker
                || userType == (int)UserTypeEnum.Manager;
        }

        private static ResponseData<object>? ValidateRolesForUserType(int userType, List<long>? roles)
        {
            if (userType == (int)UserTypeEnum.Manager)
            {
                var distinctRoles = roles?.Distinct().ToList();
                // Manager bắt buộc có ít nhất một Role
                if (distinctRoles == null || distinctRoles.Count == 0)
                {
                    return new ResponseData<object>("Người quản lý bắt buộc phải chọn Role");
                }
            }
            return null;
        }

        private async Task AssignUserRoles(long userId, int userType, List<long>? roles)
        {
            // Thí sinh không có UserRole
            if (userType != (int)UserTypeEnum.Manager)
            {
                return;
            }

            roles = roles?.Distinct().ToList();
            if (roles == null || roles.Count == 0)
            {
                return;
            }

            var userRoles = roles.Select(roleId => new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

            // Bulk insert liên kết user-role
            await _userRoleRepository.CreateListAsync(userRoles);
        }

        private Task<bool> IsSuperAdminUserAsync(long userId) =>
            _appUserRepository.IsSuperAdminUser(userId);

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix AppUser_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
