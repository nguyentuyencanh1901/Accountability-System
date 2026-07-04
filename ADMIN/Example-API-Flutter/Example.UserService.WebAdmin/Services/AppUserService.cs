using Example.Common.Enums;
using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.AppUser;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho người dùng: phân trang có lọc UserType (ApiListHelper), validate mật khẩu,
    /// chặn xóa SUPER_ADMIN, map ViewModel ↔ SaveModel với logic vai trò theo loại user.
    /// </summary>
    public class AppUserService : IAppUserService
    {
        // Repository ủy quyền gọi HTTP API người dùng
        private readonly IAppUserRepository _repository;
        // Repository vai trò — dùng nạp dropdown vai trò trên form
        private readonly IRoleRepository _roleRepository;

        /// <summary>Khởi tạo service với repository người dùng và vai trò.</summary>
        public AppUserService(IAppUserRepository repository, IRoleRepository roleRepository)
        {
            _repository = repository;
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// Danh sách phân trang; truyền UserType vào search model để lọc phía API;
        /// ApiListHelper.ParseList tách items/total/error.
        /// </summary>
        public async Task<(List<AppUserModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex, int userType)
        {
            var result = await _repository.GetListAsync(new AppUserSearchModel
            {
                PageIndex = pageIndex,
                PageSize = 10,
                UserType = userType <= 0 ? null : userType
            });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Tạo form thêm mới và nạp dropdown loại user, trạng thái, vai trò.</summary>
        public async Task<AppUserFormViewModel> BuildCreateFormAsync()
        {
            var model = new AppUserFormViewModel();
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown.</summary>
        public async Task<(AppUserFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy người dùng.");

            var model = result.Data.Adapt<AppUserFormViewModel>();
            model.IsSuperAdmin = result.Data.IsSuperAdmin;
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Validate tạo mới: bắt buộc nhập mật khẩu.</summary>
        public IEnumerable<(string Key, string Message)> ValidateCreate(AppUserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                yield return (nameof(model.Password), "Vui lòng nhập mật khẩu");
        }

        /// <summary>Map ViewModel → SaveModel (isEdit=false) và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(AppUserFormViewModel model)
            => _repository.AddAsync(ToSaveModel(model, isEdit: false));

        /// <summary>Map ViewModel → SaveModel (isEdit=true) và gọi API cập nhật.</summary>
        public async Task<ResponseData<object>> UpdateAsync(AppUserFormViewModel model)
        {
            if (model.IsSuperAdminRestrictedEdit)
            {
                var current = await _repository.GetByIdAsync(model.Id);
                if (!current.Success || current.Data == null)
                    return new ResponseData<object>(current.Message ?? "Không tìm thấy người dùng.");

                var saveModel = current.Data.Adapt<AppUserSaveModel>();
                saveModel.FullName = model.FullName;
                saveModel.Email = model.Email;
                saveModel.Phone = model.Phone;
                saveModel.PasswordHash = string.IsNullOrWhiteSpace(model.Password) ? "********" : model.Password!;
                return await _repository.UpdateAsync(saveModel);
            }

            return await _repository.UpdateAsync(ToSaveModel(model, isEdit: true));
        }

        /// <summary>Lấy form reset mật khẩu; chặn SUPER_ADMIN.</summary>
        public async Task<(ResetPasswordViewModel? Model, string? ErrorMessage)> GetResetPasswordFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy người dùng.");
            if (result.Data.IsSuperAdmin)
                return (null, "Không thể reset mật khẩu tài khoản SUPER_ADMIN");

            return (new ResetPasswordViewModel
            {
                Id = result.Data.Id,
                Username = result.Data.Username,
                FullName = result.Data.FullName
            }, null);
        }

        /// <summary>Reset mật khẩu theo mật khẩu mới do admin nhập; chặn SUPER_ADMIN.</summary>
        public async Task<(ResponseData<object>? Result, string? BlockedMessage)> ResetPasswordAsync(long id, string newPassword)
        {
            var userResult = await _repository.GetByIdAsync(id);
            if (!userResult.Success || userResult.Data == null)
                return (new ResponseData<object>(userResult.Message ?? "Không tìm thấy người dùng."), null);
            if (userResult.Data.IsSuperAdmin)
                return (null, "Không thể reset mật khẩu tài khoản SUPER_ADMIN");

            var result = await _repository.ResetPasswordAsync(new ResetPasswordModel
            {
                Id = id,
                NewPassword = newPassword
            });
            return (result, null);
        }

        /// <summary>Cập nhật nhanh trạng thái người dùng trên danh sách (Index); chặn SUPER_ADMIN.</summary>
        public async Task<(ResponseData<object>? Result, string? BlockedMessage)> UpdateStatusAsync(long id, int status)
        {
            var userResult = await _repository.GetByIdAsync(id);
            if (!userResult.Success || userResult.Data == null)
                return (new ResponseData<object>(userResult.Message ?? "Không tìm thấy người dùng."), null);
            if (userResult.Data.IsSuperAdmin)
                return (null, "Không thể đổi trạng thái tài khoản SUPER_ADMIN");

            var saveModel = userResult.Data.Adapt<AppUserSaveModel>();
            saveModel.Status = status;
            saveModel.PasswordHash = "********";
            var result = await _repository.UpdateAsync(saveModel);
            return (result, null);
        }

        /// <summary>
        /// Xóa có kiểm tra: chặn xóa tài khoản IsSuperAdmin trước khi gọi API Delete.
        /// </summary>
        public async Task<(ResponseData<object>? Result, string? BlockedMessage)> DeleteAsync(long id)
        {
            var userResult = await _repository.GetByIdAsync(id);
            if (userResult.Success && userResult.Data?.IsSuperAdmin == true)
                return (null, "Không được xóa tài khoản có quyền SUPER_ADMIN");

            var result = await _repository.DeleteAsync(id);
            return (result, null);
        }

        /// <summary>Nạp dropdown loại user, trạng thái và danh sách vai trò.</summary>
        public async Task PopulateFormOptionsAsync(AppUserFormViewModel model)
        {
            model.UserTypeOptions = SelectListHelper.UserTypes(model.UserType);
            model.StatusOptions = SelectListHelper.Statuses(model.Status);

            var rolesResult = await _roleRepository.GetListAsync(new RoleSearchModel { PageIndex = 1, PageSize = 100 });
            model.RoleOptions = rolesResult.Success && rolesResult.Data != null
                ? rolesResult.Data.Select(r => new SelectListItem(
                    r.Name, r.Id.ToString(), model.Roles.Contains(r.Id))).ToList()
                : new List<SelectListItem>();
        }

        /// <summary>
        /// Map ViewModel → SaveModel: xử lý mật khẩu (placeholder ******** khi sửa và để trống);
        /// chỉ gửi Roles khi UserType = Manager.
        /// </summary>
        private static AppUserSaveModel ToSaveModel(AppUserFormViewModel model, bool isEdit)
        {
            var saveModel = model.Adapt<AppUserSaveModel>();
            saveModel.PasswordHash = isEdit
                ? (string.IsNullOrWhiteSpace(model.Password) ? "********" : model.Password!)
                : model.Password!;
            saveModel.Roles = model.UserType == (int)UserTypeEnum.Manager ? model.Roles : null;
            return saveModel;
        }
    }
}
