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

namespace Example.UserService.API.Services
{
    /// <summary>
    /// CRUD bộ đề thi: cấu hình số câu, độ khó, tổng điểm, lĩnh vực lấy câu hỏi.
    /// Validate ngân hàng câu hỏi đủ điều kiện random. Cache Redis prefix <c>ExamSet_GetListPaging</c>.
    /// </summary>
    public class ExamSetService : IExamSetService
    {
        // Prefix Redis danh sách bộ đề
        private const string ListCacheKey = "ExamSet_GetListPaging";

        // Ghi log lỗi
        private readonly ILogger<ExamSetService> _logger;
        // CRUD ExamSet
        private readonly IExamSetRepository _examSetRepository;
        // Đồng bộ lĩnh vực gắn bộ đề
        private readonly IExamSetFieldRepository _examSetFieldRepository;
        // Validate ngân hàng câu hỏi đủ pool
        private readonly IQuestionRepository _questionRepository;
        // Báo cáo thí sinh đã thi bộ đề
        private readonly IExamSessionRepository _examSessionRepository;
        // Cache danh sách phân trang
        private readonly IRedisCache _redisCache;

        public ExamSetService(
            ILogger<ExamSetService> logger,
            IExamSetRepository examSetRepository,
            IExamSetFieldRepository examSetFieldRepository,
            IQuestionRepository questionRepository,
            IExamSessionRepository examSessionRepository,
            IRedisCache redisCache)
        {
            _logger = logger;
            _examSetRepository = examSetRepository;
            _examSetFieldRepository = examSetFieldRepository;
            _questionRepository = questionRepository;
            _examSessionRepository = examSessionRepository;
            _redisCache = redisCache;
        }

        /// <summary>Xóa bộ đề và liên kết lĩnh vực (ExamSetField).</summary>
        public async Task<ResponseData<object>> Delete(long id)
        {
            try
            {
                var entity = await _examSetRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    // business rules: xóa liên kết lĩnh vực trước
                    await _examSetFieldRepository.DeleteByExamSetIdAsync(id);
                    await _examSetRepository.DeleteAsync(entity);
                    var result = await _examSetRepository.SaveChangesAsync();
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

        /// <summary>Chi tiết bộ đề kèm danh sách FieldId.</summary>
        public async Task<ResponseData<ExamSetModel>> GetById(long id)
        {
            // validate: id hợp lệ
            if (id <= 0)
                return new ResponseData<ExamSetModel>(ErrorCodeAPI.InvalidInput);

            var entity = await _examSetRepository.GetByIdAsync(id);
            if (entity == null)
                return new ResponseData<ExamSetModel>(ErrorCodeAPI.NotFound);

            var model = entity.Adapt<ExamSetModel>();
            model.FieldIds = await _examSetFieldRepository.GetFieldIdsByExamSetIdAsync(id);
            return new ResponseData<ExamSetModel>(true, model);
        }

        /// <summary>Danh sách bộ đề phân trang; cache Redis theo filter.</summary>
        public async Task<ResponseData<IEnumerable<ExamSetModel>>> GetListPaging(ExamSetSearchModel search)
        {
            try
            {
                // cache: key theo filter + phân trang
                var cacheKey = CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey, search.Id, search.Status, search.Type, search.PageIndex, search.PageSize, search.Keyword);
                var result = await _redisCache.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalRecord = await _examSetRepository.GetTotalRecord(search);
                    if (totalRecord > 0)
                    {
                        var listPaging = await _examSetRepository.GetListPaging(search);
                        if (listPaging != null && listPaging.Any())
                        {
                            var pagedList = new PagedList<ExamSetModel>(listPaging, totalRecord, search.PageIndex, search.PageSize);
                            return new ResponseData<IEnumerable<ExamSetModel>>(true, pagedList, pagedList.GetMetaData());
                        }
                    }
                    return new ResponseData<IEnumerable<ExamSetModel>>(true);
                }, TimeSpan.FromMinutes(CacheConst.CacheDataExpireShortTime));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamSetModel>>(ex.Message);
            }
        }

        /// <summary>Tạo bộ đề mới; validate cấu hình và đồng bộ lĩnh vực trong transaction.</summary>
        public async Task<ResponseData<object>> Insert(ExamSetSaveModel model)
        {
            // transaction: examSet + examSetField
            await _examSetRepository.BeginTransactionAsync();
            try
            {
                // validate: cấu hình số câu, điểm, pool câu hỏi
                var validation = await ValidateExamSetConfigAsync(model);
                if (validation != null)
                    return new ResponseData<object>(validation);

                var data = model.Adapt<ExamSet>();
                if (data.Status <= 0)
                    data.Status = (int)StatusEnum.Active;
                if (data.Type <= 0)
                    data.Type = (int)ExamSetTypeEnum.Real;

                await _examSetRepository.CreateAsync(data);
                await _examSetRepository.SaveChangesAsync();
                await _examSetFieldRepository.SyncFieldsAsync(data.Id, model.FieldIds);
                await _examSetRepository.SaveChangesAsync();
                await _examSetRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, data);
            }
            catch (Exception ex)
            {
                await _examSetRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        /// <summary>Cập nhật bộ đề; đồng bộ lại lĩnh vực trong transaction.</summary>
        public async Task<ResponseData<object>> Update(ExamSetSaveModel model)
        {
            // transaction: cập nhật examSet + sync lĩnh vực
            await _examSetRepository.BeginTransactionAsync();
            try
            {
                // validate: cấu hình số câu, điểm, pool câu hỏi
                var validation = await ValidateExamSetConfigAsync(model);
                if (validation != null)
                    return new ResponseData<object>(validation);

                var entity = await _examSetRepository.GetByIdAsync(model.Id);
                if (entity == null || entity.Id <= 0)
                {
                    await _examSetRepository.RollbackTransactionAsync();
                    return new ResponseData<object>(ErrorCodeAPI.NotFound);
                }

                var updateModel = model.Adapt(entity);
                await _examSetRepository.UpdateAsync(updateModel);
                await _examSetFieldRepository.SyncFieldsAsync(model.Id, model.FieldIds);
                await _examSetRepository.SaveChangesAsync();
                await _examSetRepository.EndTransactionAsync();
                await InvalidateListCacheAsync();
                return new ResponseData<object>(true, updateModel);
            }
            catch (Exception ex)
            {
                await _examSetRepository.RollbackTransactionAsync();
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }

        private async Task<string?> ValidateExamSetConfigAsync(ExamSetSaveModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name)
                || model.RequiredTotalPoints <= 0
                || model.QuestionCount <= 0
                || model.DurationMinutes <= 0)
            {
                return "Dữ liệu bộ đề không hợp lệ.";
            }

            if (!IsValidExamSetType(model.Type))
                return "Loại bộ đề không hợp lệ (1=Thi thật, 2=Thi thử).";

            if (model.FieldIds == null || model.FieldIds.Count == 0)
                return "Vui lòng chọn ít nhất một lĩnh vực.";

            if (model.EasyCount + model.MediumCount + model.HardCount != model.QuestionCount)
                return "Tổng số câu Dễ + Trung bình + Khó phải bằng số câu hỏi trong đề.";

            if (model.EasyCount < 0 || model.MediumCount < 0 || model.HardCount < 0)
                return "Số câu theo độ khó không được âm.";

            if (await _examSetRepository.CheckNameExists(model.Name.Trim(), model.Id))
                return "Tên bộ đề đã tồn tại.";

            var pool = await _questionRepository.GetActiveQuestionsByFieldIdsAsync(model.FieldIds);
            return ExamQuestionSelector.ValidatePoolAvailability(
                pool,
                model.EasyCount,
                model.MediumCount,
                model.HardCount,
                model.QuestionCount,
                model.RequiredTotalPoints);
        }

        private static bool IsValidExamSetType(int type)
        {
            return type == (int)ExamSetTypeEnum.Real || type == (int)ExamSetTypeEnum.Trial;
        }

        /// <summary>Danh sách thí sinh đã thi bộ đề (không cache — báo cáo real-time).</summary>
        public async Task<ResponseData<IEnumerable<ExamSetTakerModel>>> GetExamTakers(ExamSetTakerSearchModel search)
        {
            try
            {
                // validate: examSetId hợp lệ
                if (search.ExamSetId <= 0)
                    return new ResponseData<IEnumerable<ExamSetTakerModel>>(ErrorCodeAPI.InvalidInput);

                var examSet = await _examSetRepository.GetByIdAsync(search.ExamSetId);
                if (examSet == null)
                    return new ResponseData<IEnumerable<ExamSetTakerModel>>(ErrorCodeAPI.NotFound);

                var totalRecord = await _examSessionRepository.GetTakersTotalByExamSet(search);
                if (totalRecord <= 0)
                    return new ResponseData<IEnumerable<ExamSetTakerModel>>(true);

                var list = await _examSessionRepository.GetTakersByExamSet(search);
                var pagedList = new PagedList<ExamSetTakerModel>(list, totalRecord, search.PageIndex, search.PageSize);
                return new ResponseData<IEnumerable<ExamSetTakerModel>>(true, pagedList, pagedList.GetMetaData());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<IEnumerable<ExamSetTakerModel>>(ex.Message);
            }
        }

        private Task InvalidateListCacheAsync()
            // cache: xóa prefix ExamSet_GetListPaging
            => _redisCache.RemoveCacheStartWithAsync(CacheHelper.Instance.GenerateCacheKey(0, "", ListCacheKey));
    }
}
