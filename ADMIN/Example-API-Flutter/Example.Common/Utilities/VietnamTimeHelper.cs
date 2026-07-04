using System.Globalization;

namespace Example.Common.Utilities
{
    /// <summary>Chuẩn hóa múi giờ VN (+07) giữa form, MySQL, API và hiển thị UI.</summary>
    public static class VietnamTimeHelper
    {
        public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

        /// <summary>Định dạng ngày giờ trên form và bảng danh sách.</summary>
        public const string FormInputFormat = "dd/MM/yyyy HH:mm";

        /// <summary>
        /// Gắn múi giờ VN (+07) theo giờ người dùng nhập trên form (dd/MM/yyyy HH:mm).
        /// Form gửi Unspecified → gán offset +07, không convert UTC.
        /// </summary>
        public static DateTimeOffset NormalizeFromForm(DateTimeOffset value)
        {
            var clock = DateTime.SpecifyKind(value.DateTime, DateTimeKind.Unspecified);
            return new DateTimeOffset(clock, Offset);
        }

        /// <summary>
        /// Chuẩn hóa DateTimeOffset từ API/JSON về giờ VN (+07).
        /// </summary>
        public static DateTimeOffset ToVietnamOffset(DateTimeOffset value)
        {
            if (value.Offset == Offset)
                return new DateTimeOffset(value.DateTime, Offset);

            return value.ToOffset(Offset);
        }

        /// <summary>MySQL lưu UTC dạng Unspecified → đọc ra rồi hiển thị +07.</summary>
        public static DateTimeOffset FromStorage(DateTime dbValue)
        {
            var utc = DateTime.SpecifyKind(dbValue, DateTimeKind.Unspecified);
            return new DateTimeOffset(utc, TimeSpan.Zero).ToOffset(Offset);
        }

        /// <summary>Ghi MySQL: lấy phần UTC, Kind=Unspecified (Dapper map ngược FromStorage).</summary>
        public static DateTime ToStorageDateTime(DateTimeOffset value)
            => DateTime.SpecifyKind(value.UtcDateTime, DateTimeKind.Unspecified);

        public static DateTimeOffset ToVietnamTime(DateTimeOffset value)
            => ToVietnamOffset(value);

        /// <summary>Thời điểm hiện tại theo múi giờ VN (+07).</summary>
        public static DateTimeOffset NowVietnam()
            => ToVietnamTime(DateTimeOffset.UtcNow);

        public static string ToDatetimeLocalString(DateTimeOffset value)
            => ToVietnamTime(value).ToString("yyyy-MM-dd'T'HH:mm");

        public static string ToDisplayString(DateTimeOffset value)
            => ToVietnamTime(value).ToString(FormInputFormat);

        /// <summary>Giá trị hiển thị trên input text form (cùng format danh sách).</summary>
        public static string ToFormInputString(DateTimeOffset value)
            => ToDisplayString(value);

        /// <summary>Parse chuỗi từ input form (dd/MM/yyyy HH:mm) → DateTimeOffset +07.</summary>
        public static bool TryParseFormInput(string? input, out DateTimeOffset result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var text = input.Trim();
            var formats = new[] { FormInputFormat, "yyyy-MM-dd'T'HH:mm", "yyyy-MM-ddTHH:mm" };
            if (!DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return false;

            result = new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified), Offset);
            return true;
        }
    }
}
