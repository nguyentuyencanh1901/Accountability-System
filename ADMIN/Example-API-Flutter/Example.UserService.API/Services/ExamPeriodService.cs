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
    /// Quản lý kỳ thi: đồng bộ thí sinh/bộ đề, tự đóng kỳ hết hạn, theo dõi tiến độ (GetMonitoring).
    /// Cache Redis: <c>ExamPeriod_GetListPaging</c>, <c>ExamPeriodAssignment_GetListPaging</c>.
    /// GetMonitoring không cache (real-time).
    /// </summary>
    public class ExamPeriodService : IExamPeriodService
    {
        // Prefix Redis danh sách kỳ thi
        private const string ListCacheKey = "ExamPeriod_GetListPaging";
        // Prefix Redis phân công — invalidate khi sync user/bộ đề
        private const string AssignmentListCacheKey = "ExamPeriodAssignment_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamPeriodService> _logger;
        // CRUD kỳ thi
        private readonly IExamPeriodRepository _examPeriodRepository;
        // Phân công thí sinh
        private readonly IExamPeriodAssignmentRepository _examPeriodAssignmentRepository;
        // Liên kết kỳ thi ↔ bộ đề
        private readonly IExamPeriodExamSetRepository _examPeriodExamSetRepository;
        // Validate thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Validate bộ đề
        private readonly IExamSetRepository _examSetRepository;
        // Cache danh sách
        private readonly IRedisCache _redisCache;

        public ExamPeriodService(
            ILogger<ExamPeriodService> logger,
            IExamPeriodRepository examPeriodRepository,
            IExamPeriodAssignmentRepository examPeriodAssignmentRepository,
            IExamPeriodExamSetRepository examPeriodExamSetRepository,
            IAppUserRepository appUserRepository,
            IExamSetRepository examSetRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examPeriodRepository = examPeriodRepository;
            _examPeriodAssignmentRepository = examPeriodAssignmentRepository;
            _examPeriodExamSetRepository = examPeriodExamSetRepository;
            _appUserRepository = appUserRepository;
            _examSetRepository = examSetRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa kỳ thi; chặn nếu đã có thí sinh đang thi/hoàn thành.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _examPeriodRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var assignments = await _examPeriodAssignmentRepository.GetByPeriodIdAsync(id);
                // Không xóa kỳ thi nếu đã có người thi hoặc hoàn thành
                if (assignments.Any(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.InProgress
                    || x.Status == (int)ExamPeriodAssignmentStatusEnum.Completed))
                {
                    return new ResponseData<object>("Không thể xóa kỳ thi đã có thí sinh bắt đầu hoặc hoàn thành");
                }

                // Xóa liên kết bộ đề trước (foreign key)
                var examSets = await _examPeriodExamSetRepository.GetByPeriodIdAsync(id);
                if (examSets.Count > 0)
                {
                    await _examPeriodExamSetRepository.DeleteListAsync(examSets);
                }
                // Xóa phân công thí sinh còn trạng thái Assigned
                if (assignments.Count > 0)
                {
                    await _examPeriodAssignmentRepository.DeleteListAsync(assignments);
                }

                await _examPeriodRepository.DeleteAsync(entity);
                await _examPeriodRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                await InvalidateAssignmentListCacheAsync();
                return new ResponseData<object>(true, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết kỳ thi kèm số phân công, danh sách userId và examSetId.</summary>
        public async Task<ResponseData<ExamPeriodModel>> GetById(long id)
        {
            if (id <= 0)
            {
                return new ResponseData<ExamPeriodModel>(ErrorCodeAPI.InvalidInput);
            }

            var entity = await _examPeriodRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ResponseData<ExamPeriodModel>(ErrorCodeAPI.NotFound);
            }

            var model = entity.Adapt<ExamPeriodModel>();
            // Bổ sung thông tin liên kết cho form chi tiết / edit
            model.AssignmentCount = await _examPeriodAssignmentRepository.CountByExamPeriodId(id);
            model.UserIds = await _examPeriodAssignmentRepository.GetUserIdsByPeriodIdAsync(id);
            model.ExamSetIds = await _examPeriodExamSetRepository.GetExamSetIdsByPeriodIdAsync(id);
            return new ResponseData<ExamPeriodModel>(true, model);
        }

        /// <summary>Báo cáo admin: thống kê + danh sách thí sinh kèm điểm từ ExamSession.</summary>
        public async Task<ResponseData<ExamPeriodMonitoringModel>> GetMonitoring(long id)
        {
            if (id <= 0)
            {
                return new ResponseData<ExamPeriodMonitoringModel>(ErrorCodeAPI.InvalidInput);
            }

            var entity = await _examPeriodRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ResponseData<ExamPeriodMonitoringModel>(ErrorCodeAPI.NotFound);
            }

            // Header kỳ thi + danh sách user/exam set đã gắn (cho form edit)
            var period = entity.Adapt<ExamPeriodModel>();
            period.AssignmentCount = await _examPeriodAssignmentRepository.CountByExamPeriodId(id);
            period.UserIds = await _examPeriodAssignmentRepository.GetUserIdsByPeriodIdAsync(id);
            period.ExamSetIds = await _examPeriodExamSetRepository.GetExamSetIdsByPeriodIdAsync(id);

            // JOIN ExamSessions để lấy điểm, thời gian làm bài (không cache — real-time)
            var assignments = (await _examPeriodAssignmentRepository.GetByPeriodIdWithSessionAsync(id)).ToList();
            var monitoring = new ExamPeriodMonitoringModel
            {
                Period = period,
                Assignments = assignments,
                AssignedCount = assignments.Count(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.Assigned),
                InProgressCount = assignments.Count(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.InProgress),
                CompletedCount = assignments.Count(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.Completed),
                AbsentCount = assignments.Count(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.Absent)
            };

            return new ResponseData<ExamPeriodMonitoringModel>(true, monitoring);
        }

        /// <summary>Danh sách kỳ thi phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<ExamPeriodModel>>> GetListPaging(ExamPeriodSearchModel search)
        {
            try
            {
                // Key gồm filter + phân trang — mỗi trang/filter một entry Redis
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.PageIndex, search.PageSize, search.Keyword);
                // Cache-aside: miss thì query DB và ghi Redis
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _examPeriodRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examPeriodRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamPeriodModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamPeriodModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamPeriodModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamPeriodModel>>(ex.Message);
            }
        }

        /// <summary>Tạo kỳ thi mới; đồng bộ bộ đề và phân công thí sinh trong transaction.</summary>
        public async Task<ResponseData<object>> Insert(ExamPeriodSaveModel model)
        {
            // Transaction: kỳ thi + bộ đề + phân công phải commit cùng lúc
            await _examPeriodRepository.BeginTransactionAsync();
            try
            {
                var validation = await ValidateSaveModelAsync(model);
                if (validation != null)
                {
                    await _examPeriodRepository.RollbackTransactionAsync();
                    return validation;
                }

                if (await _examPeriodRepository.CheckNameExists(model.Name, model.Id))
                {
                    await _examPeriodRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }

                var data = model.Adapt<ExamPeriod>();
                // Chuẩn hóa giờ VN trước khi lưu MySQL
                data.StartAt = VietnamTimeHelper.ToVietnamOffset(model.StartAt);
                data.EndAt = VietnamTimeHelper.ToVietnamOffset(model.EndAt);
                if (data.Status <= 0)
                {
                    data.Status = (int)ExamPeriodStatusEnum.Draft;
                }

                await _examPeriodRepository.CreateAsync(data);
                await _examPeriodRepository.SaveChangesAsync();

                // Tạo liên kết bộ đề + phân công thí sinh trong cùng transaction
                await SyncExamSetsAsync(data.Id, model.ExamSetIds);
                var examType = await ResolveExamTypeFromExamSetsAsync(model.ExamSetIds);
                await SyncUsersAsync(data.Id, model.UserIds, examType);

                await _examPeriodRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                await InvalidateAssignmentListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                await _examPeriodRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật kỳ thi; chặn sửa khi có thí sinh đang thi.</summary>
        public async Task<ResponseData<object>> Update(ExamPeriodSaveModel model)
        {
            await _examPeriodRepository.BeginTransactionAsync();
            try
            {
                // Validate trước — lỗi thì rollback, không ghi DB
                var validation = await ValidateSaveModelAsync(model);
                if (validation != null)
                {
                    await _examPeriodRepository.RollbackTransactionAsync();
                    return validation;
                }

                if (await _examPeriodRepository.CheckNameExists(model.Name, model.Id))
                {
                    await _examPeriodRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.CodeNotDuplicated);
                }

                var entity = await _examPeriodRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    await _examPeriodRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var existingAssignments = await _examPeriodAssignmentRepository.GetByPeriodIdAsync(model.Id);
                // Có thí sinh đang làm bài → không cho sửa cấu hình kỳ thi
                if (existingAssignments.Any(x => x.Status == (int)ExamPeriodAssignmentStatusEnum.InProgress))
                {
                    await _examPeriodRepository.RollbackTransactionAsync();
                    return new ResponseData<object>("Không thể sửa kỳ thi khi có thí sinh đang thi");
                }

                var updateModel = model.Adapt(entity);
                updateModel.StartAt = VietnamTimeHelper.ToVietnamOffset(model.StartAt);
                updateModel.EndAt = VietnamTimeHelper.ToVietnamOffset(model.EndAt);
                await _examPeriodRepository.UpdateAsync(updateModel);
                await _examPeriodRepository.SaveChangesAsync();

                await SyncExamSetsAsync(model.Id, model.ExamSetIds);
                var examType = await ResolveExamTypeFromExamSetsAsync(model.ExamSetIds);
                await SyncUsersAsync(model.Id, model.UserIds, examType);

                await _examPeriodRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                await InvalidateAssignmentListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                await _examPeriodRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Đồng bộ bộ đề gắn kỳ thi (thêm/xóa theo form admin).</summary>
        private async Task SyncExamSetsAsync(long examPeriodId, List<long> examSetIds)
        {
            var distinctIds = examSetIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            var existing = await _examPeriodExamSetRepository.GetByPeriodIdAsync(examPeriodId);
            var existingIds = existing.Select(x => x.ExamSetId).ToHashSet();
            var targetIds = distinctIds.ToHashSet();

            // Bộ đề bị bỏ chọn trên form → xóa khỏi ExamPeriodExamSet
            var toRemove = existing.Where(x => !targetIds.Contains(x.ExamSetId)).ToList();
            if (toRemove.Count > 0)
            {
                await _examPeriodExamSetRepository.DeleteListAsync(toRemove);
            }

            // Bộ đề mới chọn → thêm liên kết
            var toAdd = distinctIds.Where(id => !existingIds.Contains(id))
                .Select(id => new ExamPeriodExamSet { ExamPeriodId = examPeriodId, ExamSetId = id });
            if (toAdd.Any())
            {
                await _examPeriodExamSetRepository.CreateListAsync(toAdd);
            }

            await _examPeriodExamSetRepository.SaveChangesAsync();
        }

        /// <summary>Đồng bộ phân công: mỗi thí sinh chỉ có 1 loại thi theo loại bộ đề của kỳ thi.</summary>
        private async Task SyncUsersAsync(long examPeriodId, List<long> userIds, int examType)
        {
            var distinctUserIds = userIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            var existing = await _examPeriodAssignmentRepository.GetByPeriodIdAsync(examPeriodId);
            var examTypes = new List<int> { examType };

            // Chỉ xóa phân công còn Assigned (chưa thi) khi user/loại thi bị bỏ khỏi form
            foreach (var assignment in existing)
            {
                if (assignment.Status != (int)ExamPeriodAssignmentStatusEnum.Assigned)
                {
                    continue;
                }

                if (!distinctUserIds.Contains(assignment.UserId) || !examTypes.Contains(assignment.ExamType))
                {
                    await _examPeriodAssignmentRepository.DeleteAsync(assignment);
                }
            }

            // Tạo phân công mới: mỗi (user × examType), ExamSetId=null → random khi bắt đầu thi
            foreach (var userId in distinctUserIds)
            {
                foreach (var t in examTypes)
                {
                    if (!await _examPeriodAssignmentRepository.CheckAssignmentExists(examPeriodId, userId, t, 0))
                    {
                        await _examPeriodAssignmentRepository.CreateAsync(new ExamPeriodAssignment
                        {
                            ExamPeriodId = examPeriodId,
                            UserId = userId,
                            ExamSetId = null,
                            ExamType = t,
                            Status = (int)ExamPeriodAssignmentStatusEnum.Assigned
                        });
                    }
                }
            }

            await _examPeriodAssignmentRepository.SaveChangesAsync();
        }

        private async Task<int> ResolveExamTypeFromExamSetsAsync(List<long> examSetIds)
        {
            var ids = examSetIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0)
                return (int)ExamTypeEnum.Real;

            int? resolvedType = null;
            foreach (var id in ids)
            {
                var examSet = await _examSetRepository.GetByIdAsync(id);
                if (examSet == null)
                    continue;

                resolvedType ??= examSet.Type;
                if (resolvedType.Value != examSet.Type)
                {
                    // Mixed types should have been prevented by UI/validation
                    return (int)ExamTypeEnum.Real;
                }
            }

            var examSetType = resolvedType ?? (int)ExamSetTypeEnum.Real;
            return examSetType == (int)ExamSetTypeEnum.Trial ? (int)ExamTypeEnum.Trial : (int)ExamTypeEnum.Real;
        }

        private async Task<ResponseData<object>?> ValidateSaveModelAsync(ExamPeriodSaveModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
            }

            // EndAt phải sau StartAt (cùng múi giờ đã chuẩn hóa từ form)
            if (model.EndAt <= model.StartAt)
            {
                return new ResponseData<object>("Thời gian kết thúc phải sau thời gian bắt đầu");
            }

            var examSetIds = model.ExamSetIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            if (!examSetIds.Any())
            {
                return new ResponseData<object>("Vui lòng chọn ít nhất một bộ đề");
            }

            foreach (var examSetId in examSetIds)
            {
                // Chỉ cho gắn bộ đề đang Active
                var examSet = await _examSetRepository.GetByIdAsync(examSetId);
                if (examSet == null || examSet.Status != (int)StatusEnum.Active)
                {
                    return new ResponseData<object>($"Bộ đề {examSetId} không tồn tại hoặc không hoạt động");
                }
            }

            // Không cho chọn lẫn loại bộ đề trong cùng 1 kỳ thi
            var examSetTypes = new HashSet<int>();
            foreach (var examSetId in examSetIds)
            {
                var examSet = await _examSetRepository.GetByIdAsync(examSetId);
                if (examSet != null)
                    examSetTypes.Add(examSet.Type);
            }
            if (examSetTypes.Count > 1)
                return new ResponseData<object>("Kỳ thi chỉ được chọn bộ đề cùng một loại (Thi thật hoặc Thi thử).");

            var userIds = model.UserIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            if (!userIds.Any())
            {
                return new ResponseData<object>("Vui lòng chọn ít nhất một thí sinh");
            }

            foreach (var userId in userIds)
            {
                // Phải là thí sinh (TestTaker) đang hoạt động
                var user = await _appUserRepository.GetByIdAsync(userId);
                if (user == null || user.Status != (int)StatusEnum.Active
                    || user.UserType != (int)UserTypeEnum.TestTaker)
                {
                    return new ResponseData<object>($"Thí sinh {userId} không hợp lệ");
                }
            }

            return null;
        }

        /// <summary>Kỳ Published quá EndAt → Closed; phân công Assigned → Absent.</summary>
        public async Task CloseExpiredExamPeriodsAsync()
        {
            // So sánh theo giờ Việt Nam để tránh lệch timezone server
            var now = VietnamTimeHelper.ToVietnamTime(DateTimeOffset.UtcNow);
            var expiredPeriods = await _examPeriodRepository.GetPublishedExpiredAsync(now);
            if (expiredPeriods.Count == 0)
                return;

            // Duyệt từng kỳ hết hạn: đóng kỳ + đánh dấu vắng thi
            foreach (var period in expiredPeriods)
            {
                period.Status = (int)ExamPeriodStatusEnum.Closed;
                await _examPeriodRepository.UpdateAsync(period);

                var assignments = await _examPeriodAssignmentRepository.GetByPeriodIdAsync(period.Id);
                foreach (var assignment in assignments)
                {
                    // Chỉ đánh vắng những ai chưa bắt đầu thi; InProgress/Completed giữ nguyên
                    if (assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Assigned)
                    {
                        assignment.Status = (int)ExamPeriodAssignmentStatusEnum.Absent;
                        await _examPeriodAssignmentRepository.UpdateAsync(assignment);
                    }
                }
            }

            await _examPeriodRepository.SaveChangesAsync();
            await InvalidateListCacheAsync();
            await InvalidateAssignmentListCacheAsync();
        }

        // cache: xóa prefix ExamPeriod_GetListPaging
        private Task InvalidateListCacheAsync()
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));

        // cache: xóa prefix ExamPeriodAssignment_GetListPaging
        private Task InvalidateAssignmentListCacheAsync()
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", AssignmentListCacheKey));
    }
}
