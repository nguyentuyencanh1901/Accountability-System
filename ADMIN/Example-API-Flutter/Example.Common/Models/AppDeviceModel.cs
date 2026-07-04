namespace Example.Common.Models
{
    /// <summary>
    /// AppDevice Model
    /// </summary>
    public class AppDeviceModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// CustomerId
        /// </summary>
        public long CustomerId { get; set; }
        /// <summary>
        /// UserId
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// DeviceId
        /// </summary>
        public string? DeviceId { get; set; }
        /// <summary>
        /// DeviceName
        /// </summary>
        public string? DeviceName { get; set; }
        /// <summary>
        ///Device Token
        /// </summary>
        public string? DeviceToken { get; set; }
        /// <summary>
        /// IsLogin
        /// </summary>
        public bool IsLogin { get; set; }
        /// <summary>
        /// CreatedDate
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
    }
}
