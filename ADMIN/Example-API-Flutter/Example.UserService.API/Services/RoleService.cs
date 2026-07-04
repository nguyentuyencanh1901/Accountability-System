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
    /// CRUD vai trò (Role) và đồng bộ danh sách permission gắn kèm.
    /// Cache Redis prefix <c>Role_GetListPaging</c>.
    /// </summary>
    public class RoleService : IRoleService
    {
        // Prefix Redis danh sách role
        private const string ListCacheKey = "Role_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<RoleService> _logger;
        // CRUD Role
        private readonly IRoleRepository _roleRepository;
        // Đồng bộ RolePermission khi CUD role
        private readonly IRolePermissionRepository _rolePermissionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public RoleService(ILogger<RoleService> logger, IRoleRepository roleRepository, IRolePermissionRepository rolePermissionRepository, IRedisCache redisCache)
        {
            _logger = logger;
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _redisCache = redisCache;
        }
        /// <summary>Xóa role và cascade RolePermission trong transaction.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            // Transaction: xóa RolePermission trước, sau đó xóa Role
            await _roleRepository.BeginTransactionAsync();
            try
            {
                var entity = await _roleRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    // Xóa hết permission gắn với role này
                    var exist = await _rolePermissionRepository.GetById(id);
                    if (exist.Count > 0)
                    {
                        await _rolePermissionRepository.DeleteListAsync(exist);
                    }
                    await _roleRepository.DeleteAsync(entity);
                    await _roleRepository.SaveChangesAsync();
                    await _roleRepository.EndTransactionAsync();
                    // cache: xóa prefix danh sách
                    await InvalidateListCacheAsync();
                   return new ResponseData<object>(true, id);
                   
                   
                }
                return new ResponseData<object>(ErrorCodeAPI.NotFound);

            }
            catch (Exception ex)
            {
                await _roleRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết role kèm danh sách PermissionId đã gán.</summary>
        public async Task<ResponseData<RoleModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
            {
                return new ResponseData<RoleModel>(ErrorCodeAPI.InvalidInput);
            }
            var entity = await _roleRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ResponseData<RoleModel>(ErrorCodeAPI.NotFound);
            }
            var model = entity.Adapt<RoleModel>();

            // business rules: load permission đã gán cho form edit
            var users = await _rolePermissionRepository
                .GetByListId(new List<long> { id });

            model.Permissions = users.Select(x => x.PermissionId).ToList();
            model.PermissionCount = model.Permissions.Count;
            return new ResponseData<RoleModel>(true, model);
        }

        /// <summary>Danh sách role phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<RoleModel>>> GetListPaging(RoleSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.PageIndex, search.PageSize, search.Keyword);
                // Cache danh sách role theo filter phân trang
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _roleRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _roleRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<RoleModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<RoleModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<RoleModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<RoleModel>>(ex.Message);
            }
        }

        /// <summary>Tạo role mới kèm danh sách permission trong transaction.</summary>
        public async Task<ResponseData<object>> Insert(RoleSaveModel model)
        {
            // transaction: role + rolePermission
            await _roleRepository.BeginTransactionAsync();
            try
            {
               

                var data = model.Adapt<Role>();
                await _roleRepository.CreateAsync(data);
                await _roleRepository.SaveChangesAsync();
                model.Permissions = model.Permissions?.Distinct().ToList();
                // Tạo bản ghi RolePermission cho từng permission đã chọn
                if (model.Permissions != null && model.Permissions.Count > 0)
                {
                    var Role = model.Permissions.Select(d => new RolePermission
                    {
                        RoleId = data.Id,
                        PermissionId = d
                    });

                    await _rolePermissionRepository.CreateListAsync(Role);
                }

                await _roleRepository.SaveChangesAsync();
                await _roleRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                await _roleRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật role; đồng bộ lại toàn bộ permission (xóa cũ, gán mới).</summary>
        public async Task<ResponseData<object>> Update(RoleSaveModel model)
        {
            // transaction: cập nhật role + đồng bộ permission
            await _roleRepository.BeginTransactionAsync();
            try
            {
                
   
                var entity = await _roleRepository.GetByIdAsync(model.Id);
                if (entity != null && entity.Id > 0)
                {
                    

                    var updateModel = model.Adapt(entity);
                    await _roleRepository.UpdateAsync(updateModel);

                    #region UserRole
                    // Đồng bộ permission: xóa hết cũ, gán lại theo form
                    var existingUserRole = await _rolePermissionRepository.GetById(entity.Id);
                    if (existingUserRole.Count > 0)
                    {
                        await _rolePermissionRepository.DeleteListAsync(existingUserRole);
                    }
                    model.Permissions = model.Permissions?.Distinct().ToList();
                    if (model.Permissions != null && model.Permissions.Count > 0)
                    {
                        var Role = model.Permissions.Select(d => new RolePermission
                        {
                            RoleId = updateModel.Id,
                            PermissionId = d
                        });

                        await _rolePermissionRepository.CreateListAsync(Role);
                    }
                    #endregion

                    await _roleRepository.SaveChangesAsync();
                    await _roleRepository.EndTransactionAsync();
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

                await _roleRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix Role_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
