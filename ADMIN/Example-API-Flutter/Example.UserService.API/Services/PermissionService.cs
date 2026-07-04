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
using OfficeOpenXml.Style;

namespace Example.UserService.API.Services
{
    /// <summary>
    /// CRUD quyền hệ thống (Permission).
    /// Cache Redis prefix <c>Permission_GetListPaging</c>; CUD xóa prefix cache.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        // Prefix Redis danh sách quyền
        private const string ListCacheKey = "Permission_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<PermissionService> _logger;
        // CRUD Permission
        private readonly IPermissionRepository _permissionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;
        public PermissionService(ILogger<PermissionService> logger, IPermissionRepository permissionRepository, IRedisCache redisCache)
        {
            _logger = logger;
            _permissionRepository = permissionRepository;
            _redisCache = redisCache;
        }
        /// <summary>Xóa một quyền theo id.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _permissionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _permissionRepository.DeleteAsync(entity);
                    var result = await _permissionRepository.SaveChangesAsync();
                    if (result > 0)
                    {
                        await InvalidateListCacheAsync();
                        return new ResponseData<object>(true, result);
                    }
                    return new ResponseData<object>(ErrorCodeAPI.NotOk);
                }
                return new ResponseData<object>(ErrorCodeAPI.NotFound);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Lấy chi tiết quyền theo id.</summary>
        public async Task<ResponseData<PermissionModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
            {
                return new ResponseData<PermissionModel>(ErrorCodeAPI.InvalidInput);
            }
            var entity = await _permissionRepository.GetByIdAsync(id);
            if (entity != null)
            {
                return new ResponseData<PermissionModel>(true, entity.Adapt<PermissionModel>());
            }
         
            return new ResponseData<PermissionModel>(ErrorCodeAPI.NotFound);
        }

        /// <summary>Danh sách quyền phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<PermissionModel>>> GetListPaging(PermissionSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.PageIndex, search.PageSize, search.Keyword);
                // Redis cache-aside cho danh sách quyền
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _permissionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _permissionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<PermissionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<PermissionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<PermissionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<PermissionModel>>(ex.Message);
            }
        }

        /// <summary>Tạo quyền mới; validate Code unique.</summary>
        public async Task<ResponseData<object>> Insert(PermissionSaveModel model)
        {
            try
            {
                // Code permission phải unique
                if (string.IsNullOrWhiteSpace(model.Code))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (await _permissionRepository.CheckNameExists(model.Code, model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                var data = model.Adapt<Permission>();
                await _permissionRepository.CreateAsync(data);
                var result = await _permissionRepository.SaveChangesAsync();
                if (result > 0)
                {
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

        /// <summary>Cập nhật quyền; validate Code unique.</summary>
        public async Task<ResponseData<object>> Update(PermissionSaveModel model)
        {
            try
            {
                // Code permission phải unique
                if (string.IsNullOrWhiteSpace(model.Code))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (await _permissionRepository.CheckNameExists(model.Code, model.Id))
                {
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }
                var entity = await _permissionRepository.GetByIdAsync(model.Id);
                if (entity != null && entity.Id > 0)
                {
                    var updateModel = model.Adapt(entity);
                    await _permissionRepository.UpdateAsync(updateModel);
                    await _permissionRepository.SaveChangesAsync();
                    await InvalidateListCacheAsync();
                    return new ResponseData<object>(true, updateModel);
                }
                else
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix Permission_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
