using Example.Common.Enums;

namespace Example.Common.Utilities
{
    public static class DisplayHelper
    {
        public static string StatusName(int? status) => status switch
        {
            (int)StatusEnum.Active => "Hoạt động",
            (int)StatusEnum.InActive => "Ngừng hoạt động",
            _ => "—"
        };

        public static string EnumName<TEnum>(int value) where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
                return value.ToString();
            return StringUtils.GetEnumDescription((TEnum)Enum.ToObject(typeof(TEnum), value));
        }
    }
}
