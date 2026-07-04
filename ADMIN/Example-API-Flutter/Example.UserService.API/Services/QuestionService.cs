using Example.Common.Cache;
using Example.Common.Const;
using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Utilities;
using Example.Common.Utilities.Helper;
using Example.Common.Utilities.Helper;
using Example.UserService.API.Entities;
using Example.UserService.API.Models;
using Example.UserService.API.Repository.IRepository;
using Example.UserService.API.Services.IServices;
using Mapster;

namespace Example.UserService.API.Services
{
    /// <summary>
    /// Nghiệp vụ CRUD câu hỏi trắc nghiệm kèm đáp án (AnswerOption).
    /// Tạo/sửa/xóa trong transaction; khi CUD xóa cache Redis prefix
    /// <c>Question_GetListPaging</c> và <c>AnswerOption_GetListPaging</c>.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        // Prefix Redis cho danh sách phân trang câu hỏi
        private const string ListCacheKey = "Question_GetListPaging";
        // Prefix Redis đáp án — invalidate kèm khi câu hỏi thay đổi
        private const string AnswerOptionListCacheKey = "AnswerOption_GetListPaging";

        // Ghi log lỗi nghiệp vụ
        private readonly ILogger<QuestionService> _logger;
        // Truy cập bảng Question
        private readonly IQuestionRepository _questionRepository;
        // Truy cập đáp án gắn câu hỏi
        private readonly IAnswerOptionRepository _answerOptionRepository;
        // Kiểm tra đáp án đã dùng trong phiên thi (chặn xóa)
        private readonly IExamSessionAnswerOptionRepository _examSessionAnswerOptionRepository;
        // Validate lĩnh vực câu hỏi
        private readonly IFieldRepository _fieldRepository;
        // Cache-aside danh sách phân trang
        private readonly IRedisCache _redisCache;

        public QuestionService(
            ILogger<QuestionService> logger,
            IQuestionRepository questionRepository,
            IAnswerOptionRepository answerOptionRepository,
            IExamSessionAnswerOptionRepository examSessionAnswerOptionRepository,
            IFieldRepository fieldRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _questionRepository = questionRepository;
            _answerOptionRepository = answerOptionRepository;
            _examSessionAnswerOptionRepository = examSessionAnswerOptionRepository;
            _fieldRepository = fieldRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa câu hỏi và cascade đáp án con; xóa file ảnh nếu có.</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            // transaction: xóa đáp án → câu hỏi → commit
            await _questionRepository.BeginTransactionAsync();
            try
            {
                var entity = await _questionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    // Xóa cascade: đáp án con trước, câu hỏi sau
                    var options = await _answerOptionRepository.GetByQuestionId(id);
                    if (options.Count > 0)
                    {
                        await _answerOptionRepository.DeleteListAsync(options);
                    }
                    EntityImageHelper.DeleteIfExists(entity.ImageUrl);
                    await _questionRepository.DeleteAsync(entity);
                    await _questionRepository.SaveChangesAsync();
                    await _questionRepository.EndTransactionAsync();
                    // cache: xóa prefix Question + AnswerOption
                    await InvalidateQuestionCachesAsync();
                    return new ResponseData<object>(true, id);
                }
                return new ResponseData<object>(ErrorCodeAPI.NotFound);
            }
            catch (Exception ex)
            {
                await _questionRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Lấy chi tiết câu hỏi kèm tên lĩnh vực và danh sách đáp án.</summary>
        public async Task<ResponseData<QuestionModel>> GetById(long id)
        {
            try
            {
                // validate: id hợp lệ
                if (id <= 0)
                {
                    return new ResponseData<QuestionModel>(ErrorCodeAPI.InvalidInput);
                }
                var entity = await _questionRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new ResponseData<QuestionModel>(ErrorCodeAPI.NotFound);
                }
                var model = entity.Adapt<QuestionModel>();
                var field = await _fieldRepository.GetByIdAsync(model.FieldId);
                model.FieldName = field?.Name;
                // Load kèm danh sách đáp án cho form edit
                var options = await _answerOptionRepository.GetByQuestionId(id);
                model.AnswerOptions = options.Adapt<List<AnswerOptionSaveModel>>();
                return new ResponseData<QuestionModel>(true, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<QuestionModel>(ex.Message);
            }
        }

        /// <summary>Danh sách phân trang câu hỏi; cache Redis theo bộ filter.</summary>
        public async Task<ResponseData<IEnumerable<QuestionModel>>> GetListPaging(QuestionSearchModel search)
        {
            try
            {
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.FieldId, search.QuestionType, search.DifficultyLevel, search.PageIndex, search.PageSize, search.Keyword);
                // Cache danh sách câu hỏi theo bộ đề / loại câu
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _questionRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _questionRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<QuestionModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<QuestionModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<QuestionModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<QuestionModel>>(ex.Message);
            }
        }

        /// <summary>Tạo câu hỏi mới kèm đáp án và ảnh minh họa (nếu có).</summary>
        public async Task<ResponseData<object>> Insert(QuestionSaveModel model)
        {
            // transaction: câu hỏi + đáp án + ảnh
            await _questionRepository.BeginTransactionAsync();
            try
            {
                // Validate nội dung, điểm, loại câu, lĩnh vực và tối thiểu 2 đáp án có ít nhất 1 đúng
                if (string.IsNullOrWhiteSpace(model.Content) || model.Points <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (!IsValidQuestionType(model.QuestionType))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (model.FieldId <= 0)
                {
                    return new ResponseData<object>("Vui lòng chọn lĩnh vực");
                }
                var field = await _fieldRepository.GetByIdAsync(model.FieldId);
                if (field == null)
                {
                    return new ResponseData<object>("Lĩnh vực không tồn tại");
                }
                if (field.Status != (int)StatusEnum.Active)
                {
                    return new ResponseData<object>("Lĩnh vực không hoạt động");
                }
                if (!IsValidDifficultyLevel(model.DifficultyLevel))
                {
                    return new ResponseData<object>("Độ khó không hợp lệ");
                }
                if (model.AnswerOptions == null || model.AnswerOptions.Count < 2)
                {
                    return new ResponseData<object>("Câu hỏi phải có ít nhất 2 đáp án");
                }
                var correctValidation = ValidateCorrectAnswers(model.QuestionType, model.AnswerOptions);
                if (correctValidation != null)
                {
                    return new ResponseData<object>(correctValidation);
                }

                var data = model.Adapt<Question>();
                if (data.DifficultyLevel <= 0)
                {
                    data.DifficultyLevel = (int)DifficultyLevelEnum.Medium;
                }
                data.ImageUrl = null;
                await _questionRepository.CreateAsync(data);
                await _questionRepository.SaveChangesAsync();

                var imageResult = await ApplyQuestionImageAsync(data, model);
                if (imageResult != null)
                {
                    await _questionRepository.RollbackTransactionAsync();
                    return imageResult;
                }
                if (!string.IsNullOrWhiteSpace(data.ImageUrl))
                {
                    await _questionRepository.UpdateAsync(data);
                    await _questionRepository.SaveChangesAsync();
                }

                var options = model.AnswerOptions.Select((item, index) => new AnswerOption
                {
                    QuestionId = data.Id,
                    Content = item.Content,
                    IsCorrect = item.IsCorrect,
                    // SortOrder từ form hoặc theo thứ tự trong danh sách
                    SortOrder = item.SortOrder > 0 ? item.SortOrder : index + 1
                });
                await _answerOptionRepository.CreateListAsync(options);

                await _questionRepository.SaveChangesAsync();
                await _questionRepository.EndTransactionAsync();
                await InvalidateQuestionCachesAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                await _questionRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật câu hỏi; upsert đáp án; chặn xóa đáp án đã dùng trong phiên thi.</summary>
        public async Task<ResponseData<object>> Update(QuestionSaveModel model)
        {
            // transaction: cập nhật câu hỏi + đồng bộ đáp án
            await _questionRepository.BeginTransactionAsync();
            try
            {
                // Validate nội dung, điểm, loại câu, lĩnh vực và tối thiểu 2 đáp án có ít nhất 1 đúng
                if (string.IsNullOrWhiteSpace(model.Content) || model.Points <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (!IsValidQuestionType(model.QuestionType))
                {
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);
                }
                if (model.FieldId <= 0)
                {
                    return new ResponseData<object>("Vui lòng chọn lĩnh vực");
                }
                var field = await _fieldRepository.GetByIdAsync(model.FieldId);
                if (field == null)
                {
                    return new ResponseData<object>("Lĩnh vực không tồn tại");
                }
                if (field.Status != (int)StatusEnum.Active)
                {
                    return new ResponseData<object>("Lĩnh vực không hoạt động");
                }
                if (!IsValidDifficultyLevel(model.DifficultyLevel))
                {
                    return new ResponseData<object>("Độ khó không hợp lệ");
                }

                var incomingOptions = model.AnswerOptions?
                    .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                    .ToList() ?? new List<AnswerOptionSaveModel>();
                // Lọc đáp án rỗng trước khi validate số lượng
                if (incomingOptions.Count < 2)
                {
                    return new ResponseData<object>("Câu hỏi phải có ít nhất 2 đáp án");
                }
                var correctValidation = ValidateCorrectAnswers(model.QuestionType, incomingOptions);
                if (correctValidation != null)
                {
                    return new ResponseData<object>(correctValidation);
                }

                var entity = await _questionRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                var imageResult = await ApplyQuestionImageAsync(updateModel, model);
                if (imageResult != null)
                {
                    await _questionRepository.RollbackTransactionAsync();
                    return imageResult;
                }

                await _questionRepository.UpdateAsync(updateModel);

                var existingOptions = await _answerOptionRepository.GetByQuestionId(entity.Id);
                var existingById = existingOptions.ToDictionary(x => x.Id);
                var incomingIds = incomingOptions.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();

                // Upsert từng đáp án: cập nhật nếu có Id, tạo mới nếu không
                foreach (var (item, index) in incomingOptions.Select((item, index) => (item, index)))
                {
                    var sortOrder = item.SortOrder > 0 ? item.SortOrder : index + 1;
                    if (item.Id > 0 && existingById.TryGetValue(item.Id, out var existing))
                    {
                        existing.Content = item.Content;
                        existing.IsCorrect = item.IsCorrect;
                        existing.SortOrder = sortOrder;
                        await _answerOptionRepository.UpdateAsync(existing);
                    }
                    else
                    {
                        await _answerOptionRepository.CreateAsync(new AnswerOption
                        {
                            QuestionId = updateModel.Id,
                            Content = item.Content,
                            IsCorrect = item.IsCorrect,
                            SortOrder = sortOrder
                        });
                    }
                }

                var toDelete = existingOptions.Where(e => !incomingIds.Contains(e.Id)).ToList();
                // Không xóa đáp án đã dùng trong phiên thi đang/đã nộp
                foreach (var option in toDelete)
                {
                    if (await _examSessionAnswerOptionRepository.CountByAnswerOptionId(option.Id) > 0)
                    {
                        await _questionRepository.RollbackTransactionAsync();
                        return new ResponseData<object>("Không thể xóa đáp án đã được sử dụng trong phiên thi.");
                    }
                }
                if (toDelete.Count > 0)
                {
                    await _answerOptionRepository.DeleteListAsync(toDelete);
                }

                await _questionRepository.SaveChangesAsync();
                await _questionRepository.EndTransactionAsync();
                await InvalidateQuestionCachesAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                await _questionRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật nhanh trạng thái câu hỏi (Active/InActive) theo Id.
        /// Không đụng tới đáp án, chỉ thay đổi cờ Status.
        /// </summary>
        public async Task<ResponseData<object>> UpdateStatus(UpdateStatusModel model)
        {
            try
            {
                if (model.Id <= 0)
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);

                if (model.Status != (int)StatusEnum.Active && model.Status != (int)StatusEnum.InActive)
                    return new ResponseData<object>(ErrorCodeAPI.InvalidInput);

                var entity = await _questionRepository.GetByIdAsync(model.Id);
                if (entity == null)
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);

                entity.Status = model.Status;
                await _questionRepository.UpdateAsync(entity);
                await _questionRepository.SaveChangesAsync();
                await InvalidateQuestionCachesAsync();
                return new ResponseData<object>(true, entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private static bool IsValidQuestionType(int questionType)
        {
            return questionType == (int)QuestionTypeEnum.SingleChoice
                || questionType == (int)QuestionTypeEnum.MultipleChoice;
        }

        private static bool IsValidDifficultyLevel(int difficultyLevel)
        {
            return difficultyLevel == (int)DifficultyLevelEnum.Easy
                || difficultyLevel == (int)DifficultyLevelEnum.Medium
                || difficultyLevel == (int)DifficultyLevelEnum.Hard;
        }

        private static string? ValidateCorrectAnswers(int questionType, List<AnswerOptionSaveModel> options)
        {
            var correctCount = options.Count(x => x.IsCorrect);
            if (questionType == (int)QuestionTypeEnum.SingleChoice)
            {
                if (correctCount != 1)
                    return "Câu hỏi một đáp án phải có đúng 1 đáp án đúng";
            }
            else if (correctCount < 1)
            {
                return "Câu hỏi phải có ít nhất 1 đáp án đúng";
            }
            return null;
        }

        private static async Task<ResponseData<object>?> ApplyQuestionImageAsync(Question entity, QuestionSaveModel model)
        {
            var (relativePath, error) = await EntityImageHelper.ProcessAsync(
                model.ImageBase64,
                model.ImageFileName,
                model.RemoveImage,
                EntityImageHelper.Modules.Question,
                entity.Id,
                entity.ImageUrl);

            if (!string.IsNullOrEmpty(error))
            {
                return new ResponseData<object>(error);
            }

            entity.ImageUrl = relativePath;
            return null;
        }

        private async Task InvalidateQuestionCachesAsync()
        {
            // Câu hỏi thay đổi → xóa cache cả Question và AnswerOption
            await _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
            await _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", AnswerOptionListCacheKey));
        }
    }
}
