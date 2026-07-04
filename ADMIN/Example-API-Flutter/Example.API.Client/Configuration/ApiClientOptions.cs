namespace Example.API.Client.Configuration
{
    /// <summary>
    /// Cấu hình kết nối API (section appsettings: "ApiClient")
    /// </summary>
    public class ApiClientOptions
    {
        public const string SectionName = "ApiClient";

        /// <summary>URL gốc, ví dụ: http://localhost:5000</summary>
        public string BaseUrl { get; set; } = "http://localhost:5000";

        /// <summary>Phiên bản API trong route: api/v{ApiVersion}/...</summary>
        public string ApiVersion { get; set; } = "1";
    }
}
