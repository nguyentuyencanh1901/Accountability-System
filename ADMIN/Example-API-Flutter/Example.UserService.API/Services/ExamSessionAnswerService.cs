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
    /// CRUD câu trả lời theo câu hỏi trong phiên thi (ExamSessionAnswer).
    /// Cache Redis prefix <c>ExamSessionAnswer_GetListPaging</c>.
    /// </summary>
    public class ExamSessionAnswerService : IExamSessionAnswerService
    {
        // Prefix Redis danh sách câu trả lời
        private const string ListCacheKey = "ExamSessionAnswer_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamSessionAnswerService> _logger;
        // CRUD ExamSessionAnswer
        private readonly IExamSessionAnswerRepository _examSessionAnswerRepository;
        // Cascade xóa đáp án đã chọn
        private readonly IExamSessionAnswerOptionRepository _examSessionAnswerOptionRepository;
        // Validate phiên thi
        private readonly IExamSessionRepository _examSessionRepository;
        // Lấy nội dung câu hỏi
        private readonly IQuestionRepository _questionRepository;
        // Enrich thông tin thí sinh
        private readonly IAppUserRepository _appUserRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public ExamSessionAnswerService(
            ILogger<ExamSessionAnswerService> logger,
            IExamSessionAnswerRepository examSessionAnswerRepository,
            IExamSessionAnswerOptionRepository examSessionAnswerOptionRepository,
            IExamSessionRepository examSessionRepository,
            IQuestionRepository questionRepository,
            IAppUserRepository appUserRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examSessionAnswerRepository = examSessionAnswerRepository;
            _examSessionAnswerOptionRepository = examSessionAnswerOptionRepository;
            _examSessionRepository = examSessionRepository;
            _questionRepository = questionRepository;
            _appUserRepository = appUserRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa câu trả lời và cascade đáp án đã chọn trong transaction.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            // Transaction: xóa đáp án đã chọn trước, sau đó xóa câu trả lời
            await _examSessionAnswerRepository.BeginTransactionAsync();
            try
            {
                var entity = await _examSessionAnswerRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var answerOptions = await _examSessionAnswerOptionRepository.GetByExamSessionAnswerIds(new List<long> { id });
                if (answerOptions.Count > 0)
                {
                    // Cascade xóa ExamSessionAnswerOption
                    await _examSessionAnswerOptionRepository.DeleteListAsync(answerOptions);
                }
                await _examSessionAnswerRepository.DeleteAsync(entity);
                await _examSessionAnswerRepository.SaveChangesAsync();
                await _examSessionAnswerRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, id);
            }
            catch (Exception ex)
            {
                await _examSessionAnswerRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Chi tiết câu trả lời kèm nội dung câu hỏi, session và đáp án đã chọn.</summary>
        public async Task<ResponseData<ExamSessionAnswerModel>> GetById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ResponseData<ExamSessionAnswerModel>(ErrorCodeAPI.InvalidInput);
                }
                var entity = await _examSessionAnswerRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<ExamSessionAnswerModel>(ErrorCodeAPI.NotFound);
                }

                var question = await _questionRepository.GetByIdAsync(entity.QuestionId);
                var examSession = await _examSessionRepository.GetByIdAsync(entity.ExamSessionId);
                var user = examSession != null ? await _appUserRepository.GetByIdAsync(examSession.UserId) : null;
                var selectedOptions = await _examSessionAnswerOptionRepository.GetByExamSessionAnswerIds(new List<long> { id });

                // Build model kèm nội dung câu hỏi, thông tin session và đáp án đã chọn
                var model = entity.Adapt<ExamSessionAnswerModel>();
                model.QuestionContent = question?.Content;
                model.ExamSessionInfo = examSession != null ? $"Session #{examSession.Id} - {user?.FullName}" : null;
                model.SelectedAnswerOptionIds = selectedOptions.Select(x => x.AnswerOptionId).ToList();
                return new ResponseData<ExamSessionAnswerModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<ExamSessionAnswerModel>(ex.Message);
            }
        }

        /// <summary>Danh sách câu trả lời phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<ExamSessionAnswerModel>>> GetListPaging(ExamSessionAnswerSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.ExamSessionId, search.QuestionId, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _examSessionAnswerRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examSessionAnswerRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamSessionAnswerModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamSessionAnswerModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamSessionAnswerModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamSessionAnswerModel>>(ex.Message);
            }
        }

        /// <summary>Tạo câu trả lời mới cho phiên thi (admin).</summary>
        public async Task<ResponseData<object>> Insert(ExamSessionAnswerSaveModel model)
        {
            try
            {
                // Validate phiên thi và câu hỏi tồn tại trước khi tạo câu trả lời
                if (model.ExamSessionId <= 0 || model.QuestionId <= 0)
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

                var data = model.Adapt<ExamSessionAnswer>();
                await _examSessionAnswerRepository.CreateAsync(data);
                await _examSessionAnswerRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật điểm/trạng thái đúng-sai của câu trả lời.</summary>
        public async Task<ResponseData<object>> Update(ExamSessionAnswerSaveModel model)
        {
            try
            {
                // Validate phiên thi và câu hỏi tồn tại trước khi tạo câu trả lời
                if (model.ExamSessionId <= 0 || model.QuestionId <= 0)
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

                var entity = await _examSessionAnswerRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                await _examSessionAnswerRepository.UpdateAsync(updateModel);
                await _examSessionAnswerRepository.SaveChangesAsync();
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
            // cache: xóa prefix ExamSessionAnswer_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
