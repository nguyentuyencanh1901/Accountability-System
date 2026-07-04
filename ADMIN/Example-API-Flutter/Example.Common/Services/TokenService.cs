using Example.Common.Services.IServices;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Example.Common.Grpc.Services
{
    /// <summary>
    /// Dịch vụ chuyển tiếp thông tin xác thực từ HTTP sang gRPC: đọc Authorization và CustomerId từ header.
    /// Dùng khi gateway/API gọi downstream gRPC cần giữ nguyên context khách hàng.
    /// </summary>
    public class TokenService : ITokenService
    {
        // Truy cập request HTTP hiện tại để lấy header.
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>Giá trị header Authorization nguyên văn (thường là "Bearer ...").</summary>
        public string Token => (_httpContextAccessor.HttpContext?.Request?.Headers["Authorization"] ?? "").ToString();

        /// <summary>Mã khách hàng từ header CustomerId; mặc định "0" nếu thiếu.</summary>
        public string CustomerId => (_httpContextAccessor.HttpContext?.Request?.Headers["CustomerId"] ?? "0").ToString();

        /// <summary>
        /// Dựng Metadata gRPC gồm Authorization và CustomerId để gắn vào lời gọi downstream.
        /// </summary>
        public async Task<Metadata> GetHeadersForgRPC()
        {
            var headers = new Metadata();
            headers.Add("Authorization", Token);
            headers.Add("CustomerId", CustomerId);

            return headers;
        }


    }
}
