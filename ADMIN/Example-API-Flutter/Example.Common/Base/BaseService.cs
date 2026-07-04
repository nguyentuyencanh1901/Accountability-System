using Example.Common.Models;
using Microsoft.AspNetCore.Http;
using Example.Common.Services.IServices;

namespace Example.Common.Base
{
    public class BaseService
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserCommonService _userCommonService;

        public BaseService(IUserCommonService userCommonService)
        {
            _userCommonService = userCommonService;
        }

        protected Guid GetUserId()
        {
            return _userCommonService.GetUserId();
        }

    }
}
