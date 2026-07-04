using Example.Common.Cache;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Services.IServices;
using Mapster;

namespace Example.UserService.API.Services
{
    /// <summary>
    /// CRUD lĩnh vực câu hỏi (Field).
    /// Chặn xóa khi còn câu hỏi. Cache Redis prefix <c>Field_GetListPaging</c>.
    /// </summary>
    public class FieldService : IFieldService
    {
        // Prefix Redis danh sách lĩnh vực
        private const string ListCacheKey = "Field_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<FieldService> _logger;
        // CRUD Field
        private readonly IFieldRepository _fieldRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public FieldService(ILogger<FieldService> logger, IFieldRepository fieldRepository, IRedisCache redisCache)
        {
            _logger = logger;
            _fieldRepository = fieldRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa lĩnh vực; chặn nếu còn câu hỏi gắn.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _fieldRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                // business rules: không xóa lĩnh vực đang có câu hỏi
                if (await _fieldRepository.CountQuestionsByFieldIdAsync(id) > 0)
                {
                    return new ResponseData<object>("Không thể xóa lĩnh vực đang có câu hỏi");
                }

                await _fieldRepository.DeleteAsync(entity);
                var result = await _fieldRepository.SaveChangesAsync();
                if (result > 0)
                {
                    // cache: xóa prefix danh sách
                    await InvalidateListCacheAsync();
                    return new ResponseData<object>(true, id);
                }
                return new ResponseData<object>(ErrorCodeAPI.NotOk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Lấy chi tiết lĩnh vực theo id.</summary>
        public async Task<ResponseData<FieldModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
            {
                return new ResponseData<FieldModel>(ErrorCodeAPI.InvalidInput);
            }
            var entity = await _fieldRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ResponseData<FieldModel>(ErrorCodeAPI.NotFound);
            }
            return new ResponseData<FieldModel>(true, entity.Adapt<FieldModel>());
        }

        /// <summary>Danh sách lĩnh vực phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<FieldModel>>> GetListPaging(FieldSearchModel search)
        {
            try
            {
                // cache: key theo filter + phân trang
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _fieldRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _fieldRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<FieldModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<FieldModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<FieldModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<FieldModel>>(ex.Message);
            }
        }

        /// <summary>Tạo lĩnh vực mới; validate tên unique.</summary>
        public async Task<ResponseData<object>> Insert(FieldSaveModel model)
        {
            try
            {
                // validate: tên không rỗng
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (await _fieldRepository.CheckNameExists(model.Name.Trim(), model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }

                var data = model.Adapt<Field>();
                data.Name = model.Name.Trim();
                if (data.Status <= 0)
                {
                    data.Status = (int)StatusEnum.Active;
                }
                await _fieldRepository.CreateAsync(data);
                var result = await _fieldRepository.SaveChangesAsync();
                if (result > 0)
                {
                    // cache: xóa prefix danh sách
                    await InvalidateListCacheAsync();
                    return new ResponseData<object>(true, data);
                }
                return new ResponseData<object>(ErrorCodeAPI.NotOk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật lĩnh vực; validate tên unique.</summary>
        public async Task<ResponseData<object>> Update(FieldSaveModel model)
        {
            try
            {
                // validate: tên không rỗng
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (await _fieldRepository.CheckNameExists(model.Name.Trim(), model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }

                var entity = await _fieldRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                updateModel.Name = model.Name.Trim();
                await _fieldRepository.UpdateAsync(updateModel);
                await _fieldRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật nhanh trạng thái lĩnh vực (Active/InActive) theo Id.
        /// Dùng cho chỉnh trực tiếp trên danh sách (Index).
        /// </summary>
        public async Task<ResponseData<object>> UpdateStatus(UpdateStatusModel model)
        {
            try
            {
                if (model.Id <= 0)
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);

                if (model.Status != (int)StatusEnum.Active && model.Status != (int)StatusEnum.InActive)
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);

                var entity = await _fieldRepository.GetByIdAsync(model.Id);
                if (entity == null)
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);

                entity.Status = model.Status;
                await _fieldRepository.UpdateAsync(entity);
                await _fieldRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix Field_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
