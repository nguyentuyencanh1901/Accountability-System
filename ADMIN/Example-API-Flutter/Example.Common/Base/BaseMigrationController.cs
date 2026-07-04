using Example.Common.Enums;
using Example.Common.Models;
using Example.Common.Services.IServices;
using Example.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Example.Common.Base
{
    public class BaseMigrationController<TContext> : BaseController where TContext : DbContext
    {
        private readonly ILogger<BaseMigrationController<TContext>> _logger;
        private readonly IServiceProvider _serviceProvider;
        //private readonly IJobSystemConfigService _jobSystemConfigService;
        //private PackageNameTypeEnum _packageName;

        public BaseMigrationController(
            ILogger<BaseMigrationController<TContext>> logger,
            IServiceProvider serviceProvider
            //IJobSystemConfigService jobSystemConfigService,
            //PackageNameTypeEnum packageName
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            //_jobSystemConfigService = jobSystemConfigService;
            //_packageName = packageName;
        }

        //[HttpPost]
        //[Route("migration")]
        //public async virtual Task<IActionResult> MigrationAsync(long customerId)
        //{
        //    try
        //    {
        //        string packageName = StringUtils.GetEnumDisplayShortName(_packageName);
        //        string connectionString = await _jobSystemConfigService.GetConnectionString(customerId, packageName, true);
        //        return Ok(MigrationDbAsync(connectionString));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return Ok(new ResponseData<object>(ex.Message));
        //    }
        //}

        /// <summary>
        /// Migration Db
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected ResponseData<string> MigrationDb(string connectionString)
        {
            try
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    //using (var dbContext = new DataContext(connectionString))
                    using (var _dbContext = ActivatorUtilities.CreateInstance<TContext>(_serviceProvider, connectionString))
                    {
                        // Thực hiện các thay đổi và tạo migration
                        _dbContext.Database.Migrate();
                    }

                    return new ResponseData<string>(true, "Migration completed successfully.");
                }

                return new ResponseData<string>(false, "Không có chuỗi kết nối database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<string>(ex.Message);
            }
        }
    }
}
