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
    /// CRUD snapshot câu hỏi trong phiên thi (ExamSessionQuestion).
    /// Cache Redis prefix <c>ExamSessionQuestion_GetListPaging</c>.
    /// </summary>
    public class ExamSessionQuestionService : IExamSessionQuestionService
    {
        // Prefix Redis danh sách câu hỏi snapshot
        private const string ListCacheKey = "ExamSessionQuestion_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamSessionQuestionService> _logger;
        // CRUD ExamSessionQuestion
        private readonly IExamSessionQuestionRepository _examSessionQuestionRepository;
        // Validate phiên thi tồn tại
        private readonly IExamSessionRepository _examSessionRepository;
        // Lấy nội dung câu hỏi gốc
        private readonly IQuestionRepository _questionRepository;
        // Lấy đáp án gốc
        private readonly IAnswerOptionRepository _answerOptionRepository;
        // Enrich thông tin thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public ExamSessionQuestionService(
            ILogger<ExamSessionQuestionService> logger,
            IExamSessionQuestionRepository examSessionQuestionRepository,
            IExamSessionRepository examSessionRepository,
            IQuestionRepository questionRepository,
            IAnswerOptionRepository answerOptionRepository,
            IAppUserRepository appUserRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examSessionQuestionRepository = examSessionQuestionRepository;
            _examSessionRepository = examSessionRepository;
            _questionRepository = questionRepository;
            _answerOptionRepository = answerOptionRepository;
            _appUserRepository = appUserRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa một câu hỏi khỏi snapshot phiên thi (thao tác admin).</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _examSessionQuestionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    // Xóa một câu hỏi khỏi snapshot phiên thi (admin)
                    await _examSessionQuestionRepository.DeleteAsync(entity);
                    await _examSessionQuestionRepository.SaveChangesAsync();
                    // cache: xóa prefix danh sách
                    await InvalidateListCacheAsync();
                    return new ResponseData<object>(true, id);
                }
                return new ResponseData<object>(ErrorCodeAPI.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết câu hỏi snapshot kèm nội dung, đáp án và thông tin phiên thi.</summary>
        public async Task<ResponseData<ExamSessionQuestionModel>> GetById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ResponseData<ExamSessionQuestionModel>(ErrorCodeAPI.InvalidInput);
                }
                var entity = await _examSessionQuestionRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<ExamSessionQuestionModel>(ErrorCodeAPI.NotFound);
                }

                var question = await _questionRepository.GetByIdAsync(entity.QuestionId);
                var options = await _answerOptionRepository.GetByQuestionId(entity.QuestionId);
                var examSession = await _examSessionRepository.GetByIdAsync(entity.ExamSessionId);
                var user = examSession != null ? await _appUserRepository.GetByIdAsync(examSession.UserId) : null;

                // Enrich: nội dung câu hỏi, đáp án, thông tin thí sinh
                var model = entity.Adapt<ExamSessionQuestionModel>();
                model.Content = question?.Content;
                model.ImageUrl = question?.ImageUrl;
                model.QuestionType = question?.QuestionType ?? 0;
                model.AnswerOptions = options.Adapt<List<AnswerOptionSaveModel>>();
                model.ExamSessionInfo = examSession != null ? $"Session #{examSession.Id} - {user?.FullName}" : null;
                return new ResponseData<ExamSessionQuestionModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<ExamSessionQuestionModel>(ex.Message);
            }
        }

        /// <summary>Danh sách câu hỏi snapshot phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<ExamSessionQuestionModel>>> GetListPaging(ExamSessionQuestionSearchModel search)
        {
            try
            {
                // cache: key theo filter + phân trang
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.ExamSessionId, search.QuestionId, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _examSessionQuestionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examSessionQuestionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamSessionQuestionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamSessionQuestionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamSessionQuestionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamSessionQuestionModel>>(ex.Message);
            }
        }

        /// <summary>Thêm câu hỏi vào snapshot phiên thi (admin).</summary>
        public async Task<ResponseData<object>> Insert(ExamSessionQuestionSaveModel model)
        {
            try
            {
                // Validate session, câu hỏi và điểm hợp lệ
                if (model.ExamSessionId <= 0 || model.QuestionId <= 0 || model.Points <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var examSession = await _examSessionRepository.GetByIdAsync(model.ExamSessionId);
                if (examSession == null)
                {
                    return new ResponseData<object>("Phiên thi không tồn tại");
                }
                var question = await _questionRepository.GetByIdAsync(model.QuestionId);
                if (question == null)
                {
                    return new ResponseData<object>("Câu hỏi không tồn tại");
                }

                var data = model.Adapt<ExamSessionQuestion>();
                await _examSessionQuestionRepository.CreateAsync(data);
                await _examSessionQuestionRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật điểm/thứ tự câu hỏi trong snapshot.</summary>
        public async Task<ResponseData<object>> Update(ExamSessionQuestionSaveModel model)
        {
            try
            {
                // Validate session, câu hỏi và điểm hợp lệ
                if (model.ExamSessionId <= 0 || model.QuestionId <= 0 || model.Points <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var examSession = await _examSessionRepository.GetByIdAsync(model.ExamSessionId);
                if (examSession == null)
                {
                    return new ResponseData<object>("Phiên thi không tồn tại");
                }
                var question = await _questionRepository.GetByIdAsync(model.QuestionId);
                if (question == null)
                {
                    return new ResponseData<object>("Câu hỏi không tồn tại");
                }

                var entity = await _examSessionQuestionRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                await _examSessionQuestionRepository.UpdateAsync(updateModel);
                await _examSessionQuestionRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix ExamSessionQuestion_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
