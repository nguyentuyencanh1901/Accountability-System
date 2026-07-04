using Example.Common.Utilities.Helper;
using Mapster;

namespace Example.Common.Utilities
{
    public static class MapsterConfigs
    {
        public static void SetStringToDateTime(this TypeAdapterConfig typeAdapterConfig)
        {
            typeAdapterConfig
                .NewConfig<string, DateTime?>()
                .MapWith(src => string.IsNullOrEmpty(src) ? null : (DateTime?)DateTime.Parse(src))
                .IgnoreNullValues(true)
                ;
        }

        public static void SetStringIfNull(this TypeAdapterConfig typeAdapterConfig)
        {
            typeAdapterConfig
                .NewConfig<string, string>()
                .MapWith(src => string.IsNullOrEmpty(src) ? "" : src)
                .IgnoreNullValues(true)
                ;
        }
    }
}
