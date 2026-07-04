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
    /// Phân công thí sinh vào kỳ thi (ExamPeriodAssignment).
    /// WebApp dùng GetMyAssignments; cache Redis prefix <c>ExamPeriodAssignment_GetListPaging</c>.
    /// </summary>
    public class ExamPeriodAssignmentService : IExamPeriodAssignmentService
    {
        // Prefix Redis danh sách phân công
        private const string ListCacheKey = "ExamPeriodAssignment_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamPeriodAssignmentService> _logger;
        // CRUD phân công
        private readonly IExamPeriodAssignmentRepository _examPeriodAssignmentRepository;
        // Validate kỳ thi (trạng thái, đóng)
        private readonly IExamPeriodRepository _examPeriodRepository;
        // Kiểm tra bộ đề gắn kỳ thi
        private readonly IExamPeriodExamSetRepository _examPeriodExamSetRepository;
        // Validate thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Validate bộ đề
        private readonly IExamSetRepository _examSetRepository;
        // Đóng kỳ hết hạn trước GetMyAssignments
        private readonly IExamPeriodService _examPeriodService;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public ExamPeriodAssignmentService(
            ILogger<ExamPeriodAssignmentService> logger,
            IExamPeriodAssignmentRepository examPeriodAssignmentRepository,
            IExamPeriodRepository examPeriodRepository,
            IExamPeriodExamSetRepository examPeriodExamSetRepository,
            IAppUserRepository appUserRepository,
            IExamSetRepository examSetRepository,
            IExamPeriodService examPeriodService,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examPeriodAssignmentRepository = examPeriodAssignmentRepository;
            _examPeriodRepository = examPeriodRepository;
            _examPeriodExamSetRepository = examPeriodExamSetRepository;
            _appUserRepository = appUserRepository;
            _examSetRepository = examSetRepository;
            _examPeriodService = examPeriodService;
            _redisCache = redisCache;
        }

        /// <summary>Xóa phân công; chặn nếu đã InProgress hoặc Completed.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _examPeriodAssignmentRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                if (entity.Status == (int)ExamPeriodAssignmentStatusEnum.InProgress
                    || entity.Status == (int)ExamPeriodAssignmentStatusEnum.Completed)
                {
                    // Chỉ cho xóa phân công chưa bắt đầu thi
                    return new ResponseData<object>("Không thể xóa phân công đã bắt đầu hoặc đã hoàn thành");
                }

                await _examPeriodAssignmentRepository.DeleteAsync(entity);
                await _examPeriodAssignmentRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết phân công kèm thông tin join (kỳ thi, thí sinh, bộ đề).</summary>
        public async Task<ResponseData<ExamPeriodAssignmentModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
            {
                return new ResponseData<ExamPeriodAssignmentModel>(ErrorCodeAPI.InvalidInput);
            }

            var entity = await _examPeriodAssignmentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ResponseData<ExamPeriodAssignmentModel>(ErrorCodeAPI.NotFound);
            }

            // Dùng GetListPaging để lấy đủ thông tin join (tên kỳ thi, thí sinh, bộ đề)
            var search = new ExamPeriodAssignmentSearchModel
            {
                PageIndex = 1,
                PageSize = 1,
                Id = id
            };
            var list = await _examPeriodAssignmentRepository.GetListPaging(search);
            var model = list.FirstOrDefault();
            if (model == null)
            {
                // Fallback: map trực tiếp entity nếu repository không trả về
                model = entity.Adapt<ExamPeriodAssignmentModel>();
            }

            return new ResponseData<ExamPeriodAssignmentModel>(true, model);
        }

        /// <summary>Danh sách phân công phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<ExamPeriodAssignmentModel>>> GetListPaging(ExamPeriodAssignmentSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.ExamPeriodId, search.UserId, search.ExamSetId, search.ExamType, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _examPeriodAssignmentRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examPeriodAssignmentRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamPeriodAssignmentModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(ex.Message);
            }
        }

        /// <summary>Danh sách kỳ thi của thí sinh (Published/Closed); gọi CloseExpired trước khi trả về.</summary>
        public async Task<ResponseData<IEnumerable<ExamPeriodAssignmentModel>>> GetMyAssignments(long userId)
        {
            if (userId <= 0)
            {
                return new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(ErrorCodeAPI.InvalidInput);
            }

            try
            {
                // Cập nhật trạng thái kỳ thi trước khi thí sinh xem danh sách
                await _examPeriodService.CloseExpiredExamPeriodsAsync();
                var list = await _examPeriodAssignmentRepository.GetMyAssignmentsAsync(userId);
                return new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(true, list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamPeriodAssignmentModel>>(ex.Message);
            }
        }

        /// <summary>Tạo nhiều phân công theo danh sách bộ đề; bỏ qua bộ đề không hợp lệ/trùng.</summary>
        public async Task<ResponseData<object>> InsertBulk(ExamPeriodAssignmentBulkSaveModel model)
        {
            try
            {
                // Validate đầu vào bulk: kỳ thi, thí sinh, danh sách bộ đề, loại thi
                if (model.ExamPeriodId <= 0 || model.UserId <= 0 || model.ExamSetIds == null || !model.ExamSetIds.Any())
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                if (model.ExamType != (int)ExamTypeEnum.Trial && model.ExamType != (int)ExamTypeEnum.Real)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var examSetIds = model.ExamSetIds.Where(x => x > 0).Distinct().ToList();
                if (!examSetIds.Any())
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var createdCount = 0;
                var skippedMessages = new List<string>();

                // Tạo từng phân công theo bộ đề — bỏ qua bộ đề không hợp lệ hoặc trùng
                foreach (var examSetId in examSetIds)
                {
                    var saveModel = new ExamPeriodAssignmentSaveModel
                    {
                        ExamPeriodId = model.ExamPeriodId,
                        UserId = model.UserId,
                        ExamSetId = examSetId,
                        ExamType = model.ExamType,
                        Status = model.Status > 0 ? model.Status : (int)ExamPeriodAssignmentStatusEnum.Assigned
                    };

                    var validation = await ValidateSaveModelAsync(saveModel);
                    if (validation != null)
                    {
                        // Ghi nhận lý do bỏ qua, tiếp tục bộ đề tiếp theo
                        skippedMessages.Add(validation.Message ?? $"Bộ đề {examSetId}: không hợp lệ");
                        continue;
                    }

                    if (await _examPeriodAssignmentRepository.CheckAssignmentExists(
                            saveModel.ExamPeriodId, saveModel.UserId, saveModel.ExamType, 0))
                    {
                        skippedMessages.Add($"Bộ đề {examSetId} đã được phân công");
                        continue;
                    }

                    var data = saveModel.Adapt<ExamPeriodAssignment>();
                    await _examPeriodAssignmentRepository.CreateAsync(data);
                    createdCount++;
                }

                if (createdCount == 0)
                {
                    var message = skippedMessages.Any()
                        ? string.Join("; ", skippedMessages)
                        : "Không thể tạo phân công";
                    return new ResponseData<object>(message);
                }

                await _examPeriodAssignmentRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, new { CreatedCount = createdCount, Skipped = skippedMessages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Tạo một phân công thí sinh ↔ kỳ thi.</summary>
        public async Task<ResponseData<object>> Insert(ExamPeriodAssignmentSaveModel model)
        {
            try
            {
                var validation = await ValidateSaveModelAsync(model);
                if (validation != null)
                {
                    return validation;
                }

                if (await _examPeriodAssignmentRepository.CheckAssignmentExists(
                        model.ExamPeriodId, model.UserId, model.ExamType, model.Id))
                {
                    return new ResponseData<object>("Thí sinh đã được phân công bộ đề này trong kỳ thi");
                }

                var data = model.Adapt<ExamPeriodAssignment>();
                if (data.Status <= 0)
                {
                    // Mặc định trạng thái Assigned khi chưa thi
                    data.Status = (int)ExamPeriodAssignmentStatusEnum.Assigned;
                }

                await _examPeriodAssignmentRepository.CreateAsync(data);
                await _examPeriodAssignmentRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật phân công; chặn sửa khi đã Completed.</summary>
        public async Task<ResponseData<object>> Update(ExamPeriodAssignmentSaveModel model)
        {
            try
            {
                var validation = await ValidateSaveModelAsync(model);
                if (validation != null)
                {
                    return validation;
                }

                if (await _examPeriodAssignmentRepository.CheckAssignmentExists(
                        model.ExamPeriodId, model.UserId, model.ExamType, model.Id))
                {
                    return new ResponseData<object>("Thí sinh đã được phân công bộ đề này trong kỳ thi");
                }

                var entity = await _examPeriodAssignmentRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                if (entity.Status == (int)ExamPeriodAssignmentStatusEnum.Completed)
                {
                    // Phân công đã nộp bài — không cho sửa
                    return new ResponseData<object>("Không thể sửa phân công đã hoàn thành");
                }

                var updateModel = model.Adapt(entity);
                await _examPeriodAssignmentRepository.UpdateAsync(updateModel);
                await _examPeriodAssignmentRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private async Task<ResponseData<object>?> ValidateSaveModelAsync(ExamPeriodAssignmentSaveModel model)
        {
            // Kiểm tra ID kỳ thi và thí sinh hợp lệ
            if (model.ExamPeriodId <= 0 || model.UserId <= 0)
            {
                return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
            }

            if (model.ExamType != (int)ExamTypeEnum.Trial && model.ExamType != (int)ExamTypeEnum.Real)
            {
                return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
            }

            var examPeriod = await _examPeriodRepository.GetByIdAsync(model.ExamPeriodId);
            if (examPeriod == null)
            {
                return new ResponseData<object>("Kỳ thi không tồn tại");
            }

            // Kỳ đã đóng — không thêm/sửa phân công
            if (examPeriod.Status == (int)ExamPeriodStatusEnum.Closed)
            {
                return new ResponseData<object>("Kỳ thi đã đóng, không thể phân công");
            }

            // ExamType phải khớp với loại bộ đề của kỳ thi (Thi thật/Thi thử)
            var examSetIds = await _examPeriodExamSetRepository.GetExamSetIdsByPeriodIdAsync(model.ExamPeriodId);
            if (examSetIds != null && examSetIds.Count > 0)
            {
                var firstSet = await _examSetRepository.GetByIdAsync(examSetIds[0]);
                var examSetType = firstSet?.Type ?? (int)ExamSetTypeEnum.Real;
                var expectedExamType = examSetType == (int)ExamSetTypeEnum.Trial ? (int)ExamTypeEnum.Trial : (int)ExamTypeEnum.Real;
                if (model.ExamType != expectedExamType)
                {
                    return new ResponseData<object>("Loại thi không phù hợp với loại bộ đề của kỳ thi");
                }
            }

            var user = await _appUserRepository.GetByIdAsync(model.UserId);
            if (user == null || user.Status != (int)StatusEnum.Active)
            {
                return new ResponseData<object>("Thí sinh không tồn tại hoặc không hoạt động");
            }

            var examSet = model.ExamSetId > 0
                ? await _examSetRepository.GetByIdAsync(model.ExamSetId.Value)
                : null;
            // ExamSetId > 0 thì bộ đề phải tồn tại và Active
            if (model.ExamSetId > 0 && (examSet == null || examSet.Status != (int)StatusEnum.Active))
            {
                return new ResponseData<object>("Bộ đề không tồn tại hoặc không hoạt động");
            }

            return null;
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix ExamPeriodAssignment_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
