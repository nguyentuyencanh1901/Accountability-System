using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Example.Common.Utilities
{
    public static class JsonSerializerConfigs
    {
        public static JsonSerializerOptions GetOption()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public static string SerializeOriginalValues(EntityEntry entry)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in entry.OriginalValues.Properties)
            {
                dict[prop.Name] = entry.OriginalValues[prop];
            }
            return JsonSerializer.Serialize(dict);
        }

        public static string SerializeCurrentValues(EntityEntry entry)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in entry.CurrentValues.Properties)
            {
                dict[prop.Name] = entry.CurrentValues[prop];
            }
            return JsonSerializer.Serialize(dict);
        }
    }

}
