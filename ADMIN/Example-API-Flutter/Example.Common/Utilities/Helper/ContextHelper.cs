using Microsoft.AspNetCore.Http;

namespace Example.Common.Utilities.Helper
{
    public sealed class ContextHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        private static readonly Lazy<ContextHelper> lazy = new Lazy<ContextHelper>(() => new ContextHelper());
        public static ContextHelper Instance { get { return lazy.Value; } }

        private ContextHelper()
        {
            _httpContextAccessor = new HttpContextAccessor();
        }

        public void SetItem(string name, string value)
        {
            _httpContextAccessor.HttpContext?.Items.Add(name, value);
        }

        public T GetItem<T>(string name)
        {
            try
            {
                return (T)Convert.ChangeType(_httpContextAccessor.HttpContext?.Items[name] ?? default, typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default;
            }
        }

        public void SetHeader(string name, string value)
        {
            _httpContextAccessor.HttpContext?.Request.Headers.TryAdd(name, value);
        }

        public void RemoveItem(string key)
        {
            _httpContextAccessor.HttpContext?.Items.Remove(key);
        }
    }
}
