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
    /// Gán quyền (Permission) cho vai trò (Role) — bảng RolePermission.
    /// Cache Redis prefix <c>RolePermission_GetListPaging</c>.
    /// </summary>
    public class RolePermissionService : IRolePermissionService
    {
        // Prefix Redis danh sách gán quyền
        private const string ListCacheKey = "RolePermission_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<RolePermissionService> _logger;
        // CRUD RolePermission
        private readonly IRolePermissionRepository _rolePermissionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;
        public RolePermissionService(ILogger<RolePermissionService> logger, IRolePermissionRepository rolePermissionRepository, IRedisCache redisCache)
        {
            _logger = logger;
            _rolePermissionRepository = rolePermissionRepository;
            _redisCache = redisCache;
        }
        /// <summary>Xóa một bản ghi gán quyền cho role.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _rolePermissionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _rolePermissionRepository.DeleteAsync(entity);
                    var result = await _rolePermissionRepository.SaveChangesAsync();
                    if (result > 0)
                    {
                        // cache: xóa prefix danh sách
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

        /// <summary>Lấy chi tiết một bản ghi RolePermission theo id.</summary>
        public async Task<ResponseData<RolePermissionModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
            {
                return new ResponseData<RolePermissionModel>(ErrorCodeAPI.InvalidInput);
            }
            var entity = await _rolePermissionRepository.GetByIdAsync(id);
            if (entity != null)
            {
                return new ResponseData<RolePermissionModel>(true, entity.Adapt<RolePermissionModel>());
            }
         
            return new ResponseData<RolePermissionModel>(ErrorCodeAPI.NotFound);
        }

        /// <summary>Danh sách gán quyền phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<RolePermissionModel>>> GetListPaging(RolePermissionSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.PageIndex, search.PageSize, search.Keyword);
                // Redis cache-aside cho danh sách RolePermission
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _rolePermissionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _rolePermissionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<RolePermissionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<RolePermissionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<RolePermissionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<RolePermissionModel>>(ex.Message);
            }
        }

        /// <summary>Tạo bản ghi gán permission cho role.</summary>
        public async Task<ResponseData<object>> Insert(RolePermissionSaveModel model)
        {
            try
            {   
                var data = model.Adapt<RolePermission>();
                await _rolePermissionRepository.CreateAsync(data);
                var result = await _rolePermissionRepository.SaveChangesAsync();
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

        /// <summary>Cập nhật bản ghi gán permission cho role.</summary>
        public async Task<ResponseData<object>> Update(RolePermissionSaveModel model)
        {
            try
            {       
                var entity = await _rolePermissionRepository.GetByIdAsync(model.Id);
                if (entity != null && entity.Id > 0)
                {
                    var updateModel = model.Adapt(entity);
                    await _rolePermissionRepository.UpdateAsync(updateModel);
                    await _rolePermissionRepository.SaveChangesAsync();
                    // cache: xóa prefix danh sách
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
            // cache: xóa prefix RolePermission_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
