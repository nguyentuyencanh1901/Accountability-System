using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.Role;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho vai trò: phân trang (ApiListHelper), form CRUD, nạp dropdown quyền hạn,
    /// map ViewModel ↔ SaveModel.
    /// </summary>
    public class RoleService : IRoleService
    {
        // Repository ủy quyền gọi HTTP API vai trò
        private readonly IRoleRepository _repository;
        // Repository quyền hạn — dùng nạp dropdown quyền trên form vai trò
        private readonly IPermissionRepository _permissionRepository;

        /// <summary>Khởi tạo service với repository vai trò và quyền hạn.</summary>
        public RoleService(IRoleRepository repository, IPermissionRepository permissionRepository)
        {
            _repository = repository;
            _permissionRepository = permissionRepository;
        }

        /// <summary>Danh sách phân trang PageSize=10; ApiListHelper.ParseList tách items/total/error.</summary>
        public async Task<(List<RoleModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex)
        {
            var result = await _repository.GetListAsync(new RoleSearchModel { PageIndex = pageIndex, PageSize = 10 });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Tạo form thêm mới và nạp danh sách quyền hạn.</summary>
        public async Task<RoleFormViewModel> BuildCreateFormAsync()
        {
            var model = new RoleFormViewModel();
            await PopulateFormOptionsAsync(model);
            return model;
        }

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown quyền (đánh dấu đã chọn).</summary>
        public async Task<(RoleFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy vai trò.");

            var model = result.Data.Adapt<RoleFormViewModel>();
            await PopulateFormOptionsAsync(model);
            return (model, null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(RoleFormViewModel model)
            => _repository.AddAsync(model.Adapt<RoleSaveModel>());

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(RoleFormViewModel model)
            => _repository.UpdateAsync(model.Adapt<RoleSaveModel>());

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown quyền hạn; selected theo danh sách Permissions của vai trò.</summary>
        public async Task PopulateFormOptionsAsync(RoleFormViewModel model)
        {
            var selected = model.Permissions?.Where(x => x > 0).ToHashSet() ?? new HashSet<long>();
            var permResult = await _permissionRepository.GetListAsync(new PermissionSearchModel { PageIndex = 1, PageSize = 500 });
            model.PermissionOptions = permResult.Success && permResult.Data != null
                ? permResult.Data.Select(p => new SelectListItem(
                  p.Description, p.Id.ToString(), selected.Contains(p.Id))).ToList()
                : new List<SelectListItem>();
        }
    }
}
