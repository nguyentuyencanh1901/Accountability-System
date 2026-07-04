using Example.Common.Services.IServices;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Example.Common.Utilities.Helper
{
    /// <summary>
    /// Dịch vụ tiện ích user từ HTTP context: đọc JWT Bearer và trích xuất UserId (claim sub).
    /// Dùng chung cho API/microservice cần biết user đang gọi mà không parse token thủ công.
    /// </summary>
    public class UserCommonService : IUserCommonService
    {
        // Truy cập header Authorization của request hiện tại (Scoped theo HttpContext).
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserCommonService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Lấy Guid user từ claim "sub" trong JWT; trả Guid.Empty nếu không có token hợp lệ.
        /// </summary>
        public Guid GetUserId()
        {
            Guid userId = Guid.Empty;
            var jwtToken = this.GetJwtSecurityToken();

            if (jwtToken != null)
            {
                userId = jwtToken.Claims.First(claim => claim.Type.ToLower() == "sub").Value.ToGuid();
            }
            return userId;
        }

        /// <summary>Đọc và parse JWT từ header Authorization: Bearer ...</summary>
        private JwtSecurityToken? GetJwtSecurityToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                return new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            }
            return null;
        }

    }
}
