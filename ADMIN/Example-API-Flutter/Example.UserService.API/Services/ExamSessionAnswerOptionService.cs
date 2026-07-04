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
    /// CRUD đáp án thí sinh đã chọn trong phiên thi (ExamSessionAnswerOption).
    /// Cache Redis prefix <c>ExamSessionAnswerOption_GetListPaging</c>.
    /// </summary>
    public class ExamSessionAnswerOptionService : IExamSessionAnswerOptionService
    {
        // Prefix Redis danh sách đáp án đã chọn
        private const string ListCacheKey = "ExamSessionAnswerOption_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamSessionAnswerOptionService> _logger;
        // CRUD ExamSessionAnswerOption
        private readonly IExamSessionAnswerOptionRepository _examSessionAnswerOptionRepository;
        // Validate câu trả lời session
        private readonly IExamSessionAnswerRepository _examSessionAnswerRepository;
        // Lấy nội dung đáp án gốc
        private readonly IAnswerOptionRepository _answerOptionRepository;
        // Lấy nội dung câu hỏi (preview)
        private readonly IQuestionRepository _questionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public ExamSessionAnswerOptionService(
            ILogger<ExamSessionAnswerOptionService> logger,
            IExamSessionAnswerOptionRepository examSessionAnswerOptionRepository,
            IExamSessionAnswerRepository examSessionAnswerRepository,
            IAnswerOptionRepository answerOptionRepository,
            IQuestionRepository questionRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examSessionAnswerOptionRepository = examSessionAnswerOptionRepository;
            _examSessionAnswerRepository = examSessionAnswerRepository;
            _answerOptionRepository = answerOptionRepository;
            _questionRepository = questionRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa một lựa chọn đáp án khỏi câu trả lời session.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _examSessionAnswerOptionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    // Xóa một lựa chọn đáp án khỏi câu trả lời session
                    await _examSessionAnswerOptionRepository.DeleteAsync(entity);
                    await _examSessionAnswerOptionRepository.SaveChangesAsync();
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

        /// <summary>Chi tiết đáp án đã chọn kèm nội dung đáp án và preview câu hỏi.</summary>
        public async Task<ResponseData<ExamSessionAnswerOptionModel>> GetById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ResponseData<ExamSessionAnswerOptionModel>(ErrorCodeAPI.InvalidInput);
                }
                var entity = await _examSessionAnswerOptionRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<ExamSessionAnswerOptionModel>(ErrorCodeAPI.NotFound);
                }

                var answerOption = await _answerOptionRepository.GetByIdAsync(entity.AnswerOptionId);
                var examSessionAnswer = await _examSessionAnswerRepository.GetByIdAsync(entity.ExamSessionAnswerId);
                var question = examSessionAnswer != null
                    ? await _questionRepository.GetByIdAsync(examSessionAnswer.QuestionId)
                    : null;

                // Enrich model: nội dung đáp án + preview câu hỏi
                var model = entity.Adapt<ExamSessionAnswerOptionModel>();
                model.AnswerOptionContent = answerOption?.Content;
                if (examSessionAnswer != null)
                {
                    var questionPreview = question?.Content;
                    if (!string.IsNullOrEmpty(questionPreview) && questionPreview.Length > 50)
                    {
                        // Cắt ngắn nội dung câu hỏi cho hiển thị danh sách
                        questionPreview = questionPreview[..50];
                    }
                    model.ExamSessionAnswerInfo = $"Answer #{examSessionAnswer.Id} - {questionPreview}";
                }
                return new ResponseData<ExamSessionAnswerOptionModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<ExamSessionAnswerOptionModel>(ex.Message);
            }
        }

        /// <summary>Danh sách đáp án đã chọn phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<ExamSessionAnswerOptionModel>>> GetListPaging(ExamSessionAnswerOptionSearchModel search)
        {
            try
            {
                // cache: key theo filter + phân trang
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.ExamSessionAnswerId, search.AnswerOptionId, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _examSessionAnswerOptionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examSessionAnswerOptionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamSessionAnswerOptionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamSessionAnswerOptionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamSessionAnswerOptionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamSessionAnswerOptionModel>>(ex.Message);
            }
        }

        /// <summary>Gán đáp án cho câu trả lời session (thao tác admin).</summary>
        public async Task<ResponseData<object>> Insert(ExamSessionAnswerOptionSaveModel model)
        {
            try
            {
                // Validate FK: câu trả lời session và đáp án gốc phải tồn tại
                if (model.ExamSessionAnswerId <= 0 || model.AnswerOptionId <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var examSessionAnswer = await _examSessionAnswerRepository.GetByIdAsync(model.ExamSessionAnswerId);
                if (examSessionAnswer == null)
                {
                    return new ResponseData<object>("Câu trả lời phiên thi không tồn tại");
                }
                var answerOption = await _answerOptionRepository.GetByIdAsync(model.AnswerOptionId);
                if (answerOption == null)
                {
                    return new ResponseData<object>("Đáp án không tồn tại");
                }

                var data = model.Adapt<ExamSessionAnswerOption>();
                await _examSessionAnswerOptionRepository.CreateAsync(data);
                await _examSessionAnswerOptionRepository.SaveChangesAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật đáp án đã chọn trong câu trả lời session.</summary>
        public async Task<ResponseData<object>> Update(ExamSessionAnswerOptionSaveModel model)
        {
            try
            {
                // Validate FK: câu trả lời session và đáp án gốc phải tồn tại
                if (model.ExamSessionAnswerId <= 0 || model.AnswerOptionId <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var examSessionAnswer = await _examSessionAnswerRepository.GetByIdAsync(model.ExamSessionAnswerId);
                if (examSessionAnswer == null)
                {
                    return new ResponseData<object>("Câu trả lời phiên thi không tồn tại");
                }
                var answerOption = await _answerOptionRepository.GetByIdAsync(model.AnswerOptionId);
                if (answerOption == null)
                {
                    return new ResponseData<object>("Đáp án không tồn tại");
                }

                var entity = await _examSessionAnswerOptionRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                await _examSessionAnswerOptionRepository.UpdateAsync(updateModel);
                await _examSessionAnswerOptionRepository.SaveChangesAsync();
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
            // cache: xóa prefix ExamSessionAnswerOption_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
