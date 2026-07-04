using Example.Common.Models;
using Example.UserService.API.Models;
using Example.UserService.WebAdmin.Helpers;
using Example.UserService.WebAdmin.Models.Field;
using Example.UserService.WebAdmin.Repository.IRepository;
using Example.UserService.WebAdmin.Services.IServices;
using Mapster;

namespace Example.UserService.WebAdmin.Services
{
    /// <summary>
    /// Facade WebAdmin cho lĩnh vực: phân trang (ApiListHelper), form CRUD,
    /// nạp dropdown trạng thái, map ViewModel ↔ SaveModel.
    /// </summary>
    public class FieldService : IFieldService
    {
        // Repository ủy quyền gọi HTTP API lĩnh vực
        private readonly IFieldRepository _repository;

        /// <summary>Khởi tạo service với repository tương ứng.</summary>
        public FieldService(IFieldRepository repository) => _repository = repository;

        /// <summary>Danh sách phân trang PageSize=10; ApiListHelper.ParseList tách items/total/error.</summary>
        public async Task<(List<FieldModel> Items, long TotalItems, string? ErrorMessage)> GetListPagingAsync(int pageIndex)
        {
            var result = await _repository.GetListAsync(new FieldSearchModel { PageIndex = pageIndex, PageSize = 10 });
            return ApiListHelper.ParseList(result);
        }

        /// <summary>Tạo form thêm mới và nạp dropdown trạng thái.</summary>
        public FieldFormViewModel BuildCreateForm()
            => BuildFormViewModel(new FieldFormViewModel());

        /// <summary>Lấy form sửa: map API → ViewModel (Mapster), nạp dropdown trạng thái.</summary>
        public async Task<(FieldFormViewModel? Model, string? ErrorMessage)> GetEditFormAsync(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return (null, result.Message ?? "Không tìm thấy lĩnh vực.");

            return (BuildFormViewModel(result.Data.Adapt<FieldFormViewModel>()), null);
        }

        /// <summary>Map ViewModel → SaveModel và gọi API thêm.</summary>
        public Task<ResponseData<object>> InsertAsync(FieldFormViewModel model)
            => _repository.AddAsync(model.Adapt<FieldSaveModel>());

        /// <summary>Map ViewModel → SaveModel và gọi API cập nhật.</summary>
        public Task<ResponseData<object>> UpdateAsync(FieldFormViewModel model)
            => _repository.UpdateAsync(model.Adapt<FieldSaveModel>());

        /// <summary>Cập nhật nhanh trạng thái trên danh sách (Index).</summary>
        public Task<ResponseData<object>> UpdateStatusAsync(long id, int status)
            => _repository.UpdateStatusAsync(new UpdateStatusModel { Id = id, Status = status });

        /// <summary>Ủy quyền xóa qua repository.</summary>
        public Task<ResponseData<object>> DeleteAsync(long id)
            => _repository.DeleteAsync(id);

        /// <summary>Nạp dropdown trạng thái (Active/Inactive) cho form.</summary>
        public void PopulateFormOptions(FieldFormViewModel model)
            => model.StatusOptions = SelectListHelper.Statuses(model.Status);

        /// <summary>Dựng ViewModel form: nạp dropdown rồi trả về model.</summary>
        public FieldFormViewModel BuildFormViewModel(FieldFormViewModel model)
        {
            PopulateFormOptions(model);
            return model;
        }
    }
}
