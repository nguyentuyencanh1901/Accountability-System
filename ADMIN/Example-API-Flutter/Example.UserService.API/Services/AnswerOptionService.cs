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
    /// CRUD đáp án độc lập (AnswerOption).
    /// Cache Redis prefix <c>AnswerOption_GetListPaging</c>.
    /// </summary>
    public class AnswerOptionService : IAnswerOptionService
    {
        // Prefix Redis danh sách đáp án
        private const string ListCacheKey = "AnswerOption_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<AnswerOptionService> _logger;
        // CRUD đáp án
        private readonly IAnswerOptionRepository _answerOptionRepository;
        // Validate câu hỏi cha
        private readonly IQuestionRepository _questionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public AnswerOptionService(
            ILogger<AnswerOptionService> logger,
            IAnswerOptionRepository answerOptionRepository,
            IQuestionRepository questionRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _answerOptionRepository = answerOptionRepository;
            _questionRepository = questionRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa một đáp án theo id.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _answerOptionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _answerOptionRepository.DeleteAsync(entity);
                    var result = await _answerOptionRepository.SaveChangesAsync();
                    if (result > 0)
                    {
                        // cache: xóa prefix danh sách
                        await InvalidateListCacheAsync();
                        return new ResponseData<object>(true, id);
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

        /// <summary>Chi tiết đáp án kèm nội dung câu hỏi cha.</summary>
        public async Task<ResponseData<AnswerOptionModel>> GetById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ResponseData<AnswerOptionModel>(ErrorCodeAPI.InvalidInput);
                }
                var entity = await _answerOptionRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<AnswerOptionModel>(ErrorCodeAPI.NotFound);
                }
                var model = entity.Adapt<AnswerOptionModel>();
                // Bổ sung nội dung câu hỏi cha để hiển thị trên form
                var question = await _questionRepository.GetByIdAsync(entity.QuestionId);
                model.QuestionContent = question?.Content;
                return new ResponseData<AnswerOptionModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<AnswerOptionModel>(ex.Message);
            }
        }

        /// <summary>Danh sách đáp án phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<AnswerOptionModel>>> GetListPaging(AnswerOptionSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.QuestionId, search.PageIndex, search.PageSize, search.Keyword);
                // Cache phân trang theo filter QuestionId / keyword
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _answerOptionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _answerOptionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<AnswerOptionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<AnswerOptionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<AnswerOptionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<AnswerOptionModel>>(ex.Message);
            }
        }

        /// <summary>Tạo đáp án mới gắn câu hỏi.</summary>
        public async Task<ResponseData<object>> Insert(AnswerOptionSaveModel model)
        {
            try
            {
                // Validate: phải gắn câu hỏi và có nội dung đáp án
                if (model.QuestionId <= 0 || string.IsNullOrWhiteSpace(model.Content))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var question = await _questionRepository.GetByIdAsync(model.QuestionId);
                if (question == null)
                {
                    return new ResponseData<object>("Câu hỏi không tồn tại");
                }

                var data = model.Adapt<AnswerOption>();
                await _answerOptionRepository.CreateAsync(data);
                var result = await _answerOptionRepository.SaveChangesAsync();
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

        /// <summary>Cập nhật nội dung, cờ đúng/sai và thứ tự đáp án.</summary>
        public async Task<ResponseData<object>> Update(AnswerOptionSaveModel model)
        {
            try
            {
                // Validate: phải gắn câu hỏi và có nội dung đáp án
                if (model.QuestionId <= 0 || string.IsNullOrWhiteSpace(model.Content))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                var question = await _questionRepository.GetByIdAsync(model.QuestionId);
                if (question == null)
                {
                    return new ResponseData<object>("Câu hỏi không tồn tại");
                }

                var entity = await _answerOptionRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                await _answerOptionRepository.UpdateAsync(updateModel);
                await _answerOptionRepository.SaveChangesAsync();
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
            // cache: xóa prefix AnswerOption_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
