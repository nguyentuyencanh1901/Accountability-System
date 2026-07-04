namespace Example.Common.Models.AppSetting
{
    public class RateLimitingModel
    {
        public bool EnableRateLimiting { get; set; }
        public int TimeSeconds { get; set; }
        public int PermitLimit { get; set; }
    }
}
