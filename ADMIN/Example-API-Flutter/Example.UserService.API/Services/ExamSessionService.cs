using Example.Common.Cache;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Entities;
using Example.UserService.API.Helpers;
using Example.UserService.API.Models;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Services.IServices;
using Mapster;
using System.Security.Claims;

namespace Example.UserService.API.Services
{
    /// <summary>
    /// Luồng thi trắc nghiệm: bắt đầu → lưu tiến độ → nộp bài → chấm điểm.
    /// Liên kết ExamSession với ExamPeriodAssignment; tự nộp khi hết giờ (DurationMinutes).
    /// Cache Redis: <c>ExamSession_GetListPaging</c>, <c>ExamPeriodAssignment_GetListPaging</c>.
    /// </summary>
    public class ExamSessionService : IExamSessionService
    {
        // Prefix Redis danh sách phiên thi
        private const string ListCacheKey = "ExamSession_GetListPaging";
        // Prefix Redis phân công kỳ thi — invalidate khi bắt đầu/nộp bài
        private const string AssignmentListCacheKey = "ExamPeriodAssignment_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamSessionService> _logger;
        // CRUD phiên thi
        private readonly IExamSessionRepository _examSessionRepository;
        // Cấu hình bộ đề (thời gian, số câu)
        private readonly IExamSetRepository _examSetRepository;
        // Lĩnh vực lấy câu hỏi của bộ đề
        private readonly IExamSetFieldRepository _examSetFieldRepository;
        // Ngân hàng câu hỏi
        private readonly IQuestionRepository _questionRepository;
        // Đáp án gốc để chấm điểm
        private readonly IAnswerOptionRepository _answerOptionRepository;
        // Snapshot câu hỏi trong phiên thi
        private readonly IExamSessionQuestionRepository _examSessionQuestionRepository;
        // Câu trả lời đã chấm
        private readonly IExamSessionAnswerRepository _examSessionAnswerRepository;
        // Đáp án thí sinh đã tick
        private readonly IExamSessionAnswerOptionRepository _examSessionAnswerOptionRepository;
        // Validate thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Validate kỳ thi (khung giờ, trạng thái)
        private readonly IExamPeriodRepository _examPeriodRepository;
        // Phân công thí sinh ↔ kỳ thi
        private readonly IExamPeriodAssignmentRepository _examPeriodAssignmentRepository;
        // Bộ đề gắn kỳ thi (random khi bắt đầu)
        private readonly IExamPeriodExamSetRepository _examPeriodExamSetRepository;
        // Đóng kỳ thi hết hạn trước khi validate
        private readonly IExamPeriodService _examPeriodService;
        // Đọc userId từ JWT
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Cache danh sách phiên thi
        private readonly IRedisCache _redisCache;

        public ExamSessionService(
            ILogger<ExamSessionService> logger,
            IExamSessionRepository examSessionRepository,
            IExamSetRepository examSetRepository,
            IExamSetFieldRepository examSetFieldRepository,
            IQuestionRepository questionRepository,
            IAnswerOptionRepository answerOptionRepository,
            IExamSessionQuestionRepository examSessionQuestionRepository,
            IExamSessionAnswerRepository examSessionAnswerRepository,
            IExamSessionAnswerOptionRepository examSessionAnswerOptionRepository,
            IAppUserRepository appUserRepository,
            IExamPeriodRepository examPeriodRepository,
            IExamPeriodAssignmentRepository examPeriodAssignmentRepository,
            IExamPeriodExamSetRepository examPeriodExamSetRepository,
            IExamPeriodService examPeriodService,
            IHttpContextAccessor httpContextAccessor,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examSessionRepository = examSessionRepository;
            _examSetRepository = examSetRepository;
            _examSetFieldRepository = examSetFieldRepository;
            _questionRepository = questionRepository;
            _answerOptionRepository = answerOptionRepository;
            _examSessionQuestionRepository = examSessionQuestionRepository;
            _examSessionAnswerRepository = examSessionAnswerRepository;
            _examSessionAnswerOptionRepository = examSessionAnswerOptionRepository;
            _appUserRepository = appUserRepository;
            _examPeriodRepository = examPeriodRepository;
            _examPeriodAssignmentRepository = examPeriodAssignmentRepository;
            _examPeriodExamSetRepository = examPeriodExamSetRepository;
            _examPeriodService = examPeriodService;
            _httpContextAccessor = httpContextAccessor;
            _redisCache = redisCache;
        }

        /// <summary>Xóa phiên thi và cascade đáp án đã chọn, câu trả lời, câu hỏi snapshot.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            // Transaction: xóa cascade đáp án → câu trả lời → câu hỏi session → session
            await _examSessionRepository.BeginTransactionAsync();
            try
            {
                var entity = await _examSessionRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var answers = await _examSessionAnswerRepository.GetByExamSessionId(id);
                if (answers.Count > 0)
                {
                    var answerIds = answers.Select(x => x.Id).ToList();
                    // Xóa đáp án đã chọn (ExamSessionAnswerOption) trước
                    var answerOptions = await _examSessionAnswerOptionRepository.GetByExamSessionAnswerIds(answerIds);
                    if (answerOptions.Count > 0)
                    {
                        await _examSessionAnswerOptionRepository.DeleteListAsync(answerOptions);
                    }
                    await _examSessionAnswerRepository.DeleteListAsync(answers);
                }

                var sessionQuestions = await _examSessionQuestionRepository.GetByExamSessionId(id);
                if (sessionQuestions.Count > 0)
                {
                    await _examSessionQuestionRepository.DeleteListAsync(sessionQuestions);
                }

                await _examSessionRepository.DeleteAsync(entity);
                await _examSessionRepository.SaveChangesAsync();
                await _examSessionRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, id);
            }
            catch (Exception ex)
            {
                await _examSessionRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết phiên thi; tự nộp nếu quá giờ; build câu hỏi và đáp án cho client.</summary>
        public async Task<ResponseData<ExamSessionModel>> GetById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ResponseData<ExamSessionModel>(ErrorCodeAPI.InvalidInput);
                }

                var entity = await _examSessionRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<ExamSessionModel>(ErrorCodeAPI.NotFound);
                }

                // business rules: kiểm tra hết giờ trước khi trả dữ liệu
                entity = await TryAutoSubmitExpiredAsync(entity);

                var examSet = await _examSetRepository.GetByIdAsync(entity.ExamSetId);
                var model = entity.Adapt<ExamSessionModel>();
                model.ExamSetName = examSet?.Name;
                // Build câu hỏi + đáp án đã chọn cho client
                model.Questions = await BuildSessionQuestions(id, entity.Status);
                model.Answers = await BuildSessionAnswers(id);
                return new ResponseData<ExamSessionModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<ExamSessionModel>(ex.Message);
            }
        }

        /// <summary>Danh sách phiên thi phân trang; cache Redis; tự nộp session quá giờ khi lọc InProgress.</summary>
        public async Task<ResponseData<IEnumerable<ExamSessionModel>>> GetListPaging(ExamSessionSearchModel search)
        {
            try
            {
                // cache: key theo filter + phân trang
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.ExamSetId, search.UserId, search.ExamType, search.HistoryOnly, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    if (search.UserId > 0 && search.Status == (int)ExamSessionStatusEnum.InProgress)
                    {
                        // Tự nộp các session quá giờ trước khi trả danh sách
                        await ExpireOverdueSessionsAsync(search.UserId.Value);
                    }

                    var totalRecord = await _examSessionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examSessionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamSessionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamSessionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamSessionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamSessionModel>>(ex.Message);
            }
        }

        /// <summary>Tạo phiên thi thủ công (admin); mặc định trạng thái InProgress.</summary>
        public async Task<ResponseData<object>> Insert(ExamSessionSaveModel model)
        {
            try
            {
                // validate: bộ đề, user, loại thi
                if (model.ExamSetId <= 0 || model.UserId <= 0 || !IsValidExamType(model.ExamType))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var examSet = await _examSetRepository.GetByIdAsync(model.ExamSetId);
                if (examSet == null)
                {
                    return new ResponseData<object>("Bộ đề không tồn tại");
                }
                var user = await _appUserRepository.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return new ResponseData<object>("Người dùng không tồn tại");
                }

                var data = model.Adapt<ExamSession>();
                if (data.Status <= 0)
                {
                    data.Status = (int)ExamSessionStatusEnum.InProgress;
                }
                // Ghi nhận thời điểm bắt đầu làm bài
                if (data.StartedAt == default)
                {
                    data.StartedAt = DateTimeOffset.Now;
                }
                await _examSessionRepository.CreateAsync(data);
                await _examSessionRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật thông tin phiên thi (admin).</summary>
        public async Task<ResponseData<object>> Update(ExamSessionSaveModel model)
        {
            try
            {
                // validate: bộ đề, user, loại thi
                if (model.ExamSetId <= 0 || model.UserId <= 0 || !IsValidExamType(model.ExamType))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var examSet = await _examSetRepository.GetByIdAsync(model.ExamSetId);
                if (examSet == null)
                {
                    return new ResponseData<object>("Bộ đề không tồn tại");
                }
                var user = await _appUserRepository.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return new ResponseData<object>("Người dùng không tồn tại");
                }

                var entity = await _examSessionRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                await _examSessionRepository.UpdateAsync(updateModel);
                await _examSessionRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Điểm vào thi: theo phân công kỳ thi hoặc bộ đề + loại thi.</summary>
        public async Task<ResponseData<object>> StartExam(StartExamModel model)
        {
            try
            {
                // Phải có assignmentId hoặc cặp (ExamSetId + ExamType) hợp lệ
                if (!IsValidExamType(model.ExamType) && model.ExamPeriodAssignmentId <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                // Lấy user từ JWT (ClaimTypes.NameIdentifier)
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return new ResponseData<object>("Không xác định được người dùng");
                }

                ExamPeriodAssignment? assignment = null;
                // Luồng chính WebApp: bắt đầu theo phân công kỳ thi
                if (model.ExamPeriodAssignmentId > 0)
                {
                    assignment = await _examPeriodAssignmentRepository.GetUserAssignmentAsync(userId, model.ExamPeriodAssignmentId);
                    if (assignment == null)
                    {
                        return new ResponseData<object>("Không tìm thấy phân công thi");
                    }
                }
                // Luồng phụ: tìm phân công theo bộ đề + loại thi (thi thử/thi thật)
                else if (model.ExamSetId > 0 && IsValidExamType(model.ExamType))
                {
                    assignment = await _examPeriodAssignmentRepository.GetActiveByUserExamSetTypeAsync(
                        userId, model.ExamSetId, model.ExamType);
                }
                else
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                if (assignment != null)
                {
                    return await StartExamViaAssignmentAsync(userId, assignment);
                }

                // Không có phân công → thi tự do theo bộ đề (ít dùng)
                if (model.ExamSetId <= 0)
                {
                    return new ResponseData<object>("Bạn chưa được phân công bài thi này");
                }

                return await StartExamFreeAsync(userId, model.ExamSetId, model.ExamType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Bắt đầu thi qua phân công: kiểm tra kỳ thi, random bộ đề nếu chưa gán, tiếp tục session đang làm nếu có.</summary>
        private async Task<ResponseData<object>> StartExamViaAssignmentAsync(long userId, ExamPeriodAssignment assignment)
        {
            // Kiểm tra kỳ thi: Published, trong khung giờ, tự đóng kỳ hết hạn
            var periodError = await ValidateExamPeriodForStartAsync(assignment.ExamPeriodId);
            if (periodError != null)
            {
                return periodError;
            }

            var period = await _examPeriodRepository.GetByIdAsync(assignment.ExamPeriodId);
            // ExamType phải khớp với loại bộ đề của kỳ thi (Thi thật/Thi thử)
            var periodExamSetIds = await _examPeriodExamSetRepository.GetExamSetIdsByPeriodIdAsync(assignment.ExamPeriodId);
            if (periodExamSetIds != null && periodExamSetIds.Count > 0)
            {
                var firstSet = await _examSetRepository.GetByIdAsync(periodExamSetIds[0]);
                var examSetType = firstSet?.Type ?? (int)ExamSetTypeEnum.Real;
                var expectedExamType = examSetType == (int)ExamSetTypeEnum.Trial ? (int)ExamTypeEnum.Trial : (int)ExamTypeEnum.Real;
                if (assignment.ExamType != expectedExamType)
                {
                    return new ResponseData<object>("Loại thi không phù hợp với loại bộ đề của kỳ thi");
                }
            }

            if (assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Completed)
            {
                return new ResponseData<object>("Bạn đã hoàn thành bài thi được phân công");
            }

            if (assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Cancelled)
            {
                return new ResponseData<object>("Bài thi đã bị hủy do vi phạm quy chế thi. Bạn không thể làm lại.");
            }

            var examSetId = assignment.ExamSetId ?? 0;
            // Chưa gán bộ đề cố định → random 1 bộ từ danh sách kỳ thi, lưu lại để lần sau dùng cùng bộ
            if (examSetId <= 0)
            {
                var examSetIds = await _examPeriodExamSetRepository.GetExamSetIdsByPeriodIdAsync(assignment.ExamPeriodId);
                if (examSetIds == null || examSetIds.Count == 0)
                {
                    return new ResponseData<object>("Kỳ thi chưa được cấu hình bộ đề");
                }

                examSetId = examSetIds[Random.Shared.Next(examSetIds.Count)];
                assignment.ExamSetId = examSetId;
                await _examPeriodAssignmentRepository.UpdateAsync(assignment);
                await _examPeriodAssignmentRepository.SaveChangesAsync();
                await InvalidateAssignmentListCacheAsync();
            }

            var examSet = await _examSetRepository.GetByIdAsync(examSetId);
            if (examSet == null || examSet.Status != (int)StatusEnum.Active)
            {
                return new ResponseData<object>("Bộ đề phân công không tồn tại hoặc không hoạt động");
            }

            // Tìm session đang làm dở để tiếp tục (không tạo bài mới)
            var existing = await _examSessionRepository.GetInProgressByAssignmentAsync(assignment.Id);
            if (existing == null && assignment.ExamSessionId > 0)
            {
                existing = await _examSessionRepository.GetByIdAsync(assignment.ExamSessionId.Value);
                if (existing != null && existing.Status != (int)ExamSessionStatusEnum.InProgress)
                {
                    existing = null;
                }
            }

            if (existing != null)
            {
                // Hết giờ làm bài (DurationMinutes) → auto nộp trước khi quyết định tiếp tục
                existing = await TryAutoSubmitExpiredAsync(existing);
                if (existing.Status == (int)ExamSessionStatusEnum.InProgress)
                {
                    return await BuildStartExamResponseAsync(existing);
                }

                assignment = await _examPeriodAssignmentRepository.GetByIdAsync(assignment.Id);
                if (assignment == null)
                {
                    return new ResponseData<object>("Không tìm thấy phân công thi");
                }

                if (assignment.Status == (int)ExamPeriodAssignmentStatusEnum.Completed)
                {
                    return new ResponseData<object>("Bạn đã hoàn thành bài thi được phân công");
                }
            }

            // Tạo session mới + random câu hỏi
            return await CreateExamSessionAsync(
                userId,
                examSet,
                assignment.ExamType,
                assignment.Id);
        }

        private async Task<ResponseData<object>> StartExamFreeAsync(long userId, long examSetId, int examType)
        {
            var examSet = await _examSetRepository.GetByIdAsync(examSetId);
            if (examSet == null || examSet.Status != (int)StatusEnum.Active)
            {
                return new ResponseData<object>("Bộ đề không tồn tại hoặc không hoạt động");
            }

            var existing = await _examSessionRepository.GetInProgressAsync(userId, examSetId, examType);
            if (existing != null)
            {
                existing = await TryAutoSubmitExpiredAsync(existing);
                if (existing.Status == (int)ExamSessionStatusEnum.InProgress)
                {
                    return await BuildStartExamResponseAsync(existing);
                }
            }

            return await CreateExamSessionAsync(userId, examSet, examType, null);
        }

        /// <summary>Tạo session mới: random câu hỏi đạt đủ số câu + tổng điểm, gắn assignment InProgress.</summary>
        private async Task<ResponseData<object>> CreateExamSessionAsync(
            long userId,
            ExamSet examSet,
            int examType,
            long? examPeriodAssignmentId)
        {
            await _examSessionRepository.BeginTransactionAsync();
            try
            {
                var fieldIds = await _examSetFieldRepository.GetFieldIdsByExamSetIdAsync(examSet.Id);
                if (fieldIds.Count == 0)
                {
                    await _examSessionRepository.RollbackTransactionAsync();
                    return new ResponseData<object>("Bộ đề chưa cấu hình lĩnh vực lấy câu hỏi.");
                }

                var activeQuestions = await _questionRepository.GetActiveQuestionsByFieldIdsAsync(fieldIds);
                var poolError = ExamQuestionSelector.ValidatePoolAvailability(
                    activeQuestions,
                    examSet.EasyCount,
                    examSet.MediumCount,
                    examSet.HardCount,
                    examSet.QuestionCount,
                    examSet.RequiredTotalPoints);
                if (poolError != null)
                {
                    await _examSessionRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(poolError);
                }

                var selectedQuestions = ExamQuestionSelector.TrySelectRandomQuestions(
                    activeQuestions,
                    examSet.EasyCount,
                    examSet.MediumCount,
                    examSet.HardCount,
                    examSet.RequiredTotalPoints);

                if (selectedQuestions == null || selectedQuestions.Count != examSet.QuestionCount)
                {
                    await _examSessionRepository.RollbackTransactionAsync();
                    return new ResponseData<object>("Không thể chọn câu hỏi đúng số lượng, độ khó và tổng điểm yêu cầu.");
                }

                if (selectedQuestions.Sum(x => x.Points) != examSet.RequiredTotalPoints)
                {
                    await _examSessionRepository.RollbackTransactionAsync();
                    return new ResponseData<object>($"Tổng điểm câu hỏi ({selectedQuestions.Sum(x => x.Points)}) không khớp tổng điểm yêu cầu ({examSet.RequiredTotalPoints}).");
                }

                // Tạo bản ghi ExamSession
                var session = new ExamSession
                {
                    ExamSetId = examSet.Id,
                    UserId = userId,
                    ExamPeriodAssignmentId = examPeriodAssignmentId,
                    ExamType = examType,
                    StartedAt = DateTimeOffset.Now,
                    MaxScore = selectedQuestions.Sum(x => x.Points),
                    TotalScore = 0,
                    Status = (int)ExamSessionStatusEnum.InProgress
                };

                await _examSessionRepository.CreateAsync(session);
                await _examSessionRepository.SaveChangesAsync();

                // Snapshot câu hỏi vào ExamSessionQuestion (thứ tự SortOrder)
                var sessionQuestions = selectedQuestions
                    .Select((question, index) => new ExamSessionQuestion
                    {
                        ExamSessionId = session.Id,
                        QuestionId = question.Id,
                        Points = question.Points,
                        SortOrder = index + 1
                    })
                    .ToList();

                await _examSessionQuestionRepository.CreateListAsync(sessionQuestions);
                await _examSessionRepository.SaveChangesAsync();

                // Liên kết ngược: phân công → InProgress + ExamSessionId
                if (examPeriodAssignmentId > 0)
                {
                    var assignment = await _examPeriodAssignmentRepository.GetByIdAsync(examPeriodAssignmentId.Value);
                    if (assignment != null)
                    {
                        assignment.Status = (int)ExamPeriodAssignmentStatusEnum.InProgress;
                        assignment.ExamSessionId = session.Id;
                        await _examPeriodAssignmentRepository.UpdateAsync(assignment);
                        await _examPeriodAssignmentRepository.SaveChangesAsync();
                        await InvalidateAssignmentListCacheAsync();
                    }
                }

                await _examSessionRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return await BuildStartExamResponseAsync(session);
            }
            catch (Exception ex)
            {
                await _examSessionRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Kiểm tra kỳ thi Published, trong khung giờ; tự đóng kỳ hết hạn trước khi validate.</summary>
        private async Task<ResponseData<object>?> ValidateExamPeriodForStartAsync(long examPeriodId)
        {
            // Đóng các kỳ Published đã quá EndAt (và đánh dấu vắng thi)
            await _examPeriodService.CloseExpiredExamPeriodsAsync();

            var period = await _examPeriodRepository.GetByIdAsync(examPeriodId);
            if (period == null)
            {
                return new ResponseData<object>("Kỳ thi không tồn tại");
            }

            if (period.Status == (int)ExamPeriodStatusEnum.Closed)
            {
                return new ResponseData<object>("Kỳ thi đã kết thúc");
            }

            // Chỉ cho thi khi đã công bố (Draft không được vào thi)
            if (period.Status != (int)ExamPeriodStatusEnum.Published)
            {
                return new ResponseData<object>("Kỳ thi chưa được công bố hoặc đã đóng");
            }

            // So sánh theo giờ Việt Nam để tránh lệch timezone server
            var now = VietnamTimeHelper.ToVietnamTime(DateTimeOffset.UtcNow);
            var startAt = VietnamTimeHelper.ToVietnamTime(period.StartAt);
            var endAt = VietnamTimeHelper.ToVietnamTime(period.EndAt);

            if (now < startAt)
            {
                return new ResponseData<object>("Kỳ thi chưa đến thời gian bắt đầu");
            }

            if (now > endAt)
            {
                return new ResponseData<object>("Kỳ thi đã hết thời gian");
            }

            return null;
        }

        /// <summary>Lưu đáp án tạm, không chấm điểm (gradeAnswers = false).</summary>
        public async Task<ResponseData<object>> SaveExamProgress(SubmitExamModel model)
        {
            try
            {
                if (model.ExamSessionId <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var session = await _examSessionRepository.GetByIdAsync(model.ExamSessionId);
                if (session == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                session = await TryAutoSubmitExpiredAsync(session);
                if (session.Status != (int)ExamSessionStatusEnum.InProgress)
                {
                    return new ResponseData<object>("Phiên thi đã kết thúc");
                }

                var userId = GetCurrentUserId();
                if (userId > 0 && session.UserId != userId)
                {
                    return new ResponseData<object>("Không có quyền lưu bài thi này");
                }

                // Gộp đáp án đã lưu trước đó với đáp án mới từ client
                var savedAnswers = await BuildSubmitAnswersFromSessionAsync(session.Id);
                var mergedAnswers = MergeSubmitAnswers(savedAnswers, model.Answers);
                // gradeAnswers=false: chỉ persist, không tính điểm
                await PersistSessionAnswersAsync(session, mergedAnswers, gradeAnswers: false);
                return new ResponseData<object>(true, session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Nộp bài: chấm điểm, cập nhật assignment Completed.</summary>
        public async Task<ResponseData<object>> SubmitExam(SubmitExamModel model)
        {
            try
            {
                if (model.ExamSessionId <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var session = await _examSessionRepository.GetByIdAsync(model.ExamSessionId);
                if (session == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }
                session = await TryAutoSubmitExpiredAsync(session);
                if (session.Status != (int)ExamSessionStatusEnum.InProgress)
                {
                    return new ResponseData<object>("Phiên thi đã kết thúc");
                }

                var userId = GetCurrentUserId();
                if (userId > 0 && session.UserId != userId)
                {
                    return new ResponseData<object>("Không có quyền nộp bài thi này");
                }

                var savedAnswers = await BuildSubmitAnswersFromSessionAsync(session.Id);
                var mergedAnswers = MergeSubmitAnswers(savedAnswers, model.Answers);
                // Chấm điểm + Completed + cập nhật phân công
                var result = await FinalizeExamAsync(session, mergedAnswers, ExamSessionStatusEnum.Completed);
                return new ResponseData<object>(true, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>
        /// Hủy bài thi do vi phạm quy chế (chuyển tab / rời màn hình quá số lần cho phép).
        /// Chấm điểm theo đáp án đã lưu, khóa phân công không cho thi lại.
        /// </summary>
        public async Task<ResponseData<object>> CancelExamDueToViolation(SubmitExamModel model)
        {
            try
            {
                if (model.ExamSessionId <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }

                var session = await _examSessionRepository.GetByIdAsync(model.ExamSessionId);
                if (session == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                session = await TryAutoSubmitExpiredAsync(session);
                if (session.Status != (int)ExamSessionStatusEnum.InProgress)
                {
                    return new ResponseData<object>("Phiên thi đã kết thúc");
                }

                var userId = GetCurrentUserId();
                if (userId > 0 && session.UserId != userId)
                {
                    return new ResponseData<object>("Không có quyền thao tác bài thi này");
                }

                var savedAnswers = await BuildSubmitAnswersFromSessionAsync(session.Id);
                var mergedAnswers = MergeSubmitAnswers(savedAnswers, model.Answers);
                session.ViolationCount = model.ViolationCount > 0 ? model.ViolationCount : 0;
                var result = await FinalizeExamAsync(session, mergedAnswers, ExamSessionStatusEnum.Cancelled);
                return new ResponseData<object>(true, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private async Task<ResponseData<object>> BuildStartExamResponseAsync(ExamSession session)
        {
            var examSet = await _examSetRepository.GetByIdAsync(session.ExamSetId);
            var result = session.Adapt<ExamSessionModel>();
            result.ExamSetName = examSet?.Name;
            result.Questions = await BuildSessionQuestions(session.Id, session.Status);
            result.Answers = await BuildSessionAnswers(session.Id);
            return new ResponseData<object>(true, result);
        }

        /// <summary>Hết giờ làm bài theo DurationMinutes → auto nộp với trạng thái Expired.</summary>
        private async Task<ExamSession> TryAutoSubmitExpiredAsync(ExamSession session)
        {
            if (session.Status != (int)ExamSessionStatusEnum.InProgress)
            {
                return session;
            }

            var examSet = await _examSetRepository.GetByIdAsync(session.ExamSetId);
            var durationMinutes = examSet?.DurationMinutes ?? 60;
            // Còn trong thời gian làm bài → không làm gì
            if (DateTimeOffset.Now <= session.StartedAt.AddMinutes(durationMinutes))
            {
                return session;
            }

            // Quá giờ: nộp với đáp án đã lưu, trạng thái Expired (khác Completed khi tự nộp)
            var savedAnswers = await BuildSubmitAnswersFromSessionAsync(session.Id);
            await FinalizeExamAsync(session, savedAnswers, ExamSessionStatusEnum.Expired);
            return (await _examSessionRepository.GetByIdAsync(session.Id))!;
        }

        private async Task ExpireOverdueSessionsAsync(long userId)
        {
            var sessions = await _examSessionRepository.GetInProgressByUserAsync(userId);
            // Duyệt từng session đang làm — auto nộp nếu quá DurationMinutes
            foreach (var session in sessions)
            {
                await TryAutoSubmitExpiredAsync(session);
            }
        }

        private static List<SubmitAnswerModel> MergeSubmitAnswers(
            List<SubmitAnswerModel> saved,
            List<SubmitAnswerModel>? incoming)
        {
            var merged = saved.ToDictionary(
                x => x.QuestionId,
                x => x.SelectedAnswerOptionIds?.Where(id => id > 0).Distinct().ToList() ?? new List<long>());

            if (incoming != null)
            {
                // Ghi đè đáp án theo QuestionId từ payload client gửi lên
                foreach (var answer in incoming)
                {
                    merged[answer.QuestionId] = answer.SelectedAnswerOptionIds?
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList() ?? new List<long>();
                }
            }

            return merged
                .Select(x => new SubmitAnswerModel
                {
                    QuestionId = x.Key,
                    SelectedAnswerOptionIds = x.Value
                })
                .ToList();
        }

        private async Task<List<SubmitAnswerModel>> BuildSubmitAnswersFromSessionAsync(long examSessionId)
        {
            var answers = await BuildSessionAnswers(examSessionId);
            return answers.Select(a => new SubmitAnswerModel
            {
                QuestionId = a.QuestionId,
                SelectedAnswerOptionIds = a.SelectedAnswerOptionIds ?? new List<long>()
            }).ToList();
        }

        /// <summary>Chấm điểm, ghi FinishedAt, đồng bộ trạng thái phân công kỳ thi.</summary>
        private async Task<ExamSessionModel> FinalizeExamAsync(
            ExamSession session,
            List<SubmitAnswerModel> answers,
            ExamSessionStatusEnum finalStatus)
        {
            await _examSessionRepository.BeginTransactionAsync();
            try
            {
                // Chấm và lưu đáp án (gradeAnswers=true)
                await PersistSessionAnswersAsync(session, answers, gradeAnswers: true);

                var sessionAnswers = await _examSessionAnswerRepository.GetByExamSessionId(session.Id);
                session.TotalScore = sessionAnswers.Sum(x => x.Score);
                session.FinishedAt = DateTimeOffset.Now;
                session.Status = (int)finalStatus;
                await _examSessionRepository.UpdateAsync(session);
                await _examSessionRepository.SaveChangesAsync();

                // Đồng bộ trạng thái phân công kỳ thi sau khi kết thúc bài thi
                if (session.ExamPeriodAssignmentId > 0)
                {
                    var assignment = await _examPeriodAssignmentRepository.GetByIdAsync(session.ExamPeriodAssignmentId.Value);
                    if (assignment != null)
                    {
                        assignment.Status = finalStatus == ExamSessionStatusEnum.Cancelled
                            ? (int)ExamPeriodAssignmentStatusEnum.Cancelled
                            : (int)ExamPeriodAssignmentStatusEnum.Completed;
                        assignment.ExamSessionId = session.Id;
                        await _examPeriodAssignmentRepository.UpdateAsync(assignment);
                        await _examPeriodAssignmentRepository.SaveChangesAsync();
                        await InvalidateAssignmentListCacheAsync();
                    }
                }

                await _examSessionRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();

                var result = session.Adapt<ExamSessionModel>();
                result.Answers = await BuildSessionAnswers(session.Id);
                return result;
            }
            catch
            {
                await _examSessionRepository.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>Ghi đáp án session; gradeAnswers=true thì so khớp đáp án đúng và tính điểm.</summary>
        private async Task PersistSessionAnswersAsync(
            ExamSession session,
            List<SubmitAnswerModel> answers,
            bool gradeAnswers)
        {
            // Xóa đáp án cũ rồi ghi lại (tránh trùng khi lưu nhiều lần)
            await ClearSessionAnswersAsync(session.Id);

            var sessionQuestions = await _examSessionQuestionRepository.GetByExamSessionId(session.Id);
            var questionIds = sessionQuestions.Select(x => x.QuestionId).ToList();
            var allOptions = await _answerOptionRepository.GetByQuestionIds(questionIds);
            var optionsByQuestion = allOptions.GroupBy(x => x.QuestionId).ToDictionary(x => x.Key, x => x.ToList());

            var sessionAnswers = new List<ExamSessionAnswer>();
            var sessionAnswerOptions = new List<ExamSessionAnswerOption>();

            foreach (var sessionQuestion in sessionQuestions)
            {
                if (!optionsByQuestion.TryGetValue(sessionQuestion.QuestionId, out var questionOptions))
                {
                    continue;
                }

                // Lấy đáp án thí sinh gửi cho câu hỏi này
                var submitAnswer = answers.FirstOrDefault(x => x.QuestionId == sessionQuestion.QuestionId);
                var selectedIds = submitAnswer?.SelectedAnswerOptionIds?.Where(id => id > 0).Distinct().OrderBy(x => x).ToList()
                    ?? new List<long>();

                var isCorrect = false;
                var score = 0m;
                if (gradeAnswers)
                {
                    // So sánh tập đáp án đã chọn với tập đáp án đúng (thứ tự đã sort)
                    var correctIds = questionOptions.Where(x => x.IsCorrect).Select(x => x.Id).OrderBy(x => x).ToList();
                    isCorrect = correctIds.SequenceEqual(selectedIds);
                    score = isCorrect ? sessionQuestion.Points : 0;
                }

                sessionAnswers.Add(new ExamSessionAnswer
                {
                    ExamSessionId = session.Id,
                    QuestionId = sessionQuestion.QuestionId,
                    IsCorrect = isCorrect,
                    Score = score
                });
            }

            if (sessionAnswers.Count == 0)
            {
                return;
            }

            await _examSessionAnswerRepository.CreateListAsync(sessionAnswers);
            await _examSessionRepository.SaveChangesAsync();

            foreach (var sessionAnswer in sessionAnswers)
            {
                var submitAnswer = answers.FirstOrDefault(x => x.QuestionId == sessionAnswer.QuestionId);
                if (submitAnswer?.SelectedAnswerOptionIds == null)
                {
                    continue;
                }

                // Lưu từng đáp án thí sinh đã tick
                foreach (var optionId in submitAnswer.SelectedAnswerOptionIds.Where(id => id > 0).Distinct())
                {
                    sessionAnswerOptions.Add(new ExamSessionAnswerOption
                    {
                        ExamSessionAnswerId = sessionAnswer.Id,
                        AnswerOptionId = optionId
                    });
                }
            }

            if (sessionAnswerOptions.Count > 0)
            {
                await _examSessionAnswerOptionRepository.CreateListAsync(sessionAnswerOptions);
                await _examSessionRepository.SaveChangesAsync();
            }
        }

        private async Task ClearSessionAnswersAsync(long examSessionId)
        {
            var answers = await _examSessionAnswerRepository.GetByExamSessionId(examSessionId);
            if (answers.Count == 0)
            {
                return;
            }

            var answerIds = answers.Select(x => x.Id).ToList();
            var answerOptions = await _examSessionAnswerOptionRepository.GetByExamSessionAnswerIds(answerIds);
            if (answerOptions.Count > 0)
            {
                await _examSessionAnswerOptionRepository.DeleteListAsync(answerOptions);
            }

            await _examSessionAnswerRepository.DeleteListAsync(answers);
            await _examSessionRepository.SaveChangesAsync();
        }

        private async Task<List<ExamSessionQuestionModel>> BuildSessionQuestions(long examSessionId, int sessionStatus)
        {
            var sessionQuestions = await _examSessionQuestionRepository.GetByExamSessionId(examSessionId);
            if (sessionQuestions.Count == 0)
            {
                return new List<ExamSessionQuestionModel>();
            }

            var questionIds = sessionQuestions.Select(x => x.QuestionId).ToList();
            var questions = new List<Question>();
            // Load từng câu hỏi gốc theo snapshot session
            foreach (var questionId in questionIds)
            {
                var question = await _questionRepository.GetByIdAsync(questionId);
                if (question != null)
                {
                    questions.Add(question);
                }
            }

            var allOptions = await _answerOptionRepository.GetByQuestionIds(questionIds);
            var hideCorrectAnswer = sessionStatus == (int)ExamSessionStatusEnum.InProgress;
            var isCompleted = sessionStatus == (int)ExamSessionStatusEnum.Completed
                || sessionStatus == (int)ExamSessionStatusEnum.Expired
                || sessionStatus == (int)ExamSessionStatusEnum.Cancelled;

            var sessionAnswers = (await _examSessionAnswerRepository.GetByExamSessionId(examSessionId)).ToList();
            var selectedByQuestion = new Dictionary<long, List<long>>();
            if (sessionAnswers.Count > 0)
            {
                var answerIds = sessionAnswers.Select(x => x.Id).ToList();
                var selectedOptions = await _examSessionAnswerOptionRepository.GetByExamSessionAnswerIds(answerIds);
                var optionsByAnswer = selectedOptions.GroupBy(x => x.ExamSessionAnswerId)
                    .ToDictionary(x => x.Key, x => x.Select(o => o.AnswerOptionId).ToList());

                // Map QuestionId → danh sách đáp án đã chọn
                foreach (var sessionAnswer in sessionAnswers)
                {
                    selectedByQuestion[sessionAnswer.QuestionId] =
                        optionsByAnswer.TryGetValue(sessionAnswer.Id, out var ids) ? ids : new List<long>();
                }
            }

            return sessionQuestions.Select(sq =>
            {
                var question = questions.FirstOrDefault(x => x.Id == sq.QuestionId);
                var options = allOptions.Where(x => x.QuestionId == sq.QuestionId)
                    .OrderBy(x => x.SortOrder)
                    .Adapt<List<AnswerOptionSaveModel>>();

                if (hideCorrectAnswer)
                {
                    // Đang thi: ẩn cờ IsCorrect để thí sinh không biết đáp án đúng
                    foreach (var option in options)
                    {
                        option.IsCorrect = false;
                    }
                }

                var answer = sessionAnswers.FirstOrDefault(x => x.QuestionId == sq.QuestionId);

                return new ExamSessionQuestionModel
                {
                    Id = sq.Id,
                    ExamSessionId = sq.ExamSessionId,
                    QuestionId = sq.QuestionId,
                    Points = sq.Points,
                    SortOrder = sq.SortOrder,
                    Content = question?.Content,
                    ImageUrl = question?.ImageUrl,
                    QuestionType = question?.QuestionType ?? 0,
                    AnswerOptions = options,
                    IsAnswerCorrect = isCompleted ? answer?.IsCorrect : null,
                    AnswerScore = isCompleted ? answer?.Score : null,
                    SelectedAnswerOptionIds = selectedByQuestion.TryGetValue(sq.QuestionId, out var selectedIds)
                        ? selectedIds
                        : new List<long>()
                };
            }).ToList();
        }

        private async Task<List<ExamSessionAnswerModel>> BuildSessionAnswers(long examSessionId)
        {
            var answers = await _examSessionAnswerRepository.GetByExamSessionId(examSessionId);
            if (answers.Count == 0)
            {
                return new List<ExamSessionAnswerModel>();
            }

            var answerIds = answers.Select(x => x.Id).ToList();
            var selectedOptions = await _examSessionAnswerOptionRepository.GetByExamSessionAnswerIds(answerIds);
            var optionsByAnswer = selectedOptions.GroupBy(x => x.ExamSessionAnswerId)
                .ToDictionary(x => x.Key, x => x.Select(o => o.AnswerOptionId).ToList());

            return answers.Select(answer => new ExamSessionAnswerModel
            {
                Id = answer.Id,
                ExamSessionId = answer.ExamSessionId,
                QuestionId = answer.QuestionId,
                IsCorrect = answer.IsCorrect,
                Score = answer.Score,
                SelectedAnswerOptionIds = optionsByAnswer.TryGetValue(answer.Id, out var ids) ? ids : new List<long>()
            }).ToList();
        }

        private long GetCurrentUserId()
        {
            // Đọc userId từ claim JWT đã gắn khi đăng nhập
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            return 0;
        }

        private static bool IsValidExamType(int examType)
        {
            return examType == (int)ExamTypeEnum.Trial || examType == (int)ExamTypeEnum.Real;
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix ExamSession_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));

        private Task InvalidateAssignmentListCacheAsync()
            // cache: xóa prefix ExamPeriodAssignment_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", AssignmentListCacheKey));
    }
}
