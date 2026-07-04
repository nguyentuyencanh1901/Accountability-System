using Dapper;
using System.Data;

namespace Example.Common.Utilities
{
    public class VietnamDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            if (value is DateTime dateTime)
                return VietnamTimeHelper.FromStorage(dateTime);

            if (value is DateTimeOffset dateTimeOffset)
                return VietnamTimeHelper.ToVietnamOffset(dateTimeOffset);

            throw new DataException($"Cannot convert {value?.GetType()} to DateTimeOffset");
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
            => parameter.Value = VietnamTimeHelper.ToStorageDateTime(value);
    }

    public class VietnamNullableDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
    {
        public override DateTimeOffset? Parse(object value)
        {
            if (value is null or DBNull)
                return null;

            if (value is DateTime dateTime)
                return VietnamTimeHelper.FromStorage(dateTime);

            if (value is DateTimeOffset dateTimeOffset)
                return VietnamTimeHelper.ToVietnamOffset(dateTimeOffset);

            throw new DataException($"Cannot convert {value.GetType()} to DateTimeOffset?");
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
            => parameter.Value = value.HasValue ? VietnamTimeHelper.ToStorageDateTime(value.Value) : DBNull.Value;
    }

    /// <summary>Đăng ký Dapper TypeHandler để đọc/ghi DateTimeOffset đúng giờ VN từ MySQL.</summary>
    public static class DapperVietnamTimeConfiguration
    {
        private static bool _registered;

        public static void Register()
        {
            if (_registered)
                return;

            SqlMapper.AddTypeHandler(new VietnamDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(new VietnamNullableDateTimeOffsetHandler());
            _registered = true;
        }
    }
}
