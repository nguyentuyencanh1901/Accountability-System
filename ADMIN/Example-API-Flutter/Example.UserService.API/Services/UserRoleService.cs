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
    /// Gán vai trò (Role) cho người quản lý (UserRole).
    /// Cache Redis prefix <c>UserRole_GetListPaging</c>.
    /// </summary>
    public class UserRoleService : IUserRoleService
    {
        // Prefix Redis danh sách gán role
        private const string ListCacheKey = "UserRole_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<UserRoleService> _logger;
        // CRUD UserRole
        private readonly IUserRoleRepository _userRoleRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;
        public UserRoleService(ILogger<UserRoleService> logger, IUserRoleRepository userRoleRepository, IRedisCache redisCache)
        {
            _logger = logger;
            _userRoleRepository = userRoleRepository;
            _redisCache = redisCache;
        }
        /// <summary>Xóa một bản ghi gán role cho user.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _userRoleRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _userRoleRepository.DeleteAsync(entity);
                    var result = await _userRoleRepository.SaveChangesAsync();
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

        /// <summary>Xuất danh sách UserRole ra file Excel (dùng dữ liệu có cache).</summary>
        public async Task<byte[]> ExportUserRoleExcel(UserRoleSearchModel model)
        {
            // Xuất Excel từ cùng nguồn dữ liệu phân trang (có cache)
            var data = await GetListPaging(model);
            if (data?.Data == null || !data.Data.Any())
            {
                throw new Exception("Không có dữ liệu");
            }


            var columns = new List<ExcelColumnConfig>
                {
                    new ExcelColumnConfig
                    {
                        Header = "Mã danh mục",
                        PropertyName = "Id"
                    },
                    new ExcelColumnConfig
                    {
                        Header = "Tên danh mục",
                        PropertyName = "UserRoleName"
                    },
                    new ExcelColumnConfig
                    {
                        Header = "SEO URL",
                        PropertyName = "Slug"
                    },
                    new ExcelColumnConfig
                    {
                        Header = "Danh mục cha",
                        PropertyName = "ParentId",
                        Style = c => c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center
                    },
                    new ExcelColumnConfig
                    {
                        Header = "Cấp",
                        PropertyName = "Level",
                        Style = c => c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center
                    },
                    new ExcelColumnConfig
                    {
                        Header = "Đường dẫn",
                        PropertyName = "Path"
                    },
                    new ExcelColumnConfig
                    {
                        Header = "Trạng thái",
                        PropertyName = "IsActive",

                        // ✅ format value
                        ValueFormatter = v => (bool)v ? "Hiển thị" : "Ẩn",

                        // ✅ style
                        Style = c =>
                        {
                            c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            var value = c.Value?.ToString();
                            if (value == "Hiển thị")
                                c.Style.Font.Color.SetColor(System.Drawing.Color.Green);
                            else
                                c.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }
                    }
                };


            return ExcelHelper.Export(data.Data.ToList(), "UserRole", "Danh Sách Danh Mục", columns);

        }

        /// <summary>Lấy chi tiết một bản ghi UserRole theo id.</summary>
        public async Task<ResponseData<UserRoleModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
            {
                return new ResponseData<UserRoleModel>(ErrorCodeAPI.InvalidInput);
            }
            var entity = await _userRoleRepository.GetByIdAsync(id);
            if (entity != null)
            {
                return new ResponseData<UserRoleModel>(true, entity.Adapt<UserRoleModel>());
            }
         
            return new ResponseData<UserRoleModel>(ErrorCodeAPI.NotFound);
        }

        /// <summary>Danh sách gán role phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<UserRoleModel>>> GetListPaging(UserRoleSearchModel search)
        {
            try
            {
                // Key cache theo filter + phân trang
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _userRoleRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _userRoleRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<UserRoleModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<UserRoleModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<UserRoleModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<UserRoleModel>>(ex.Message);
            }
        }

        /// <summary>Tạo bản ghi gán role cho user.</summary>
        public async Task<ResponseData<object>> Insert(UserRoleSaveModel model)
        {
            try
            {   
                var data = model.Adapt<UserRole>();
                await _userRoleRepository.CreateAsync(data);
                var result = await _userRoleRepository.SaveChangesAsync();
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

        /// <summary>Cập nhật bản ghi gán role cho user.</summary>
        public async Task<ResponseData<object>> Update(UserRoleSaveModel model)
        {
            try
            {       
                var entity = await _userRoleRepository.GetByIdAsync(model.Id);
                if (entity != null && entity.Id > 0)
                {
                    var updateModel = model.Adapt(entity);
                    await _userRoleRepository.UpdateAsync(updateModel);
                    await _userRoleRepository.SaveChangesAsync();
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
            // cache: xóa prefix UserRole_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
