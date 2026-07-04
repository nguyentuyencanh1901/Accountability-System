using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Cache
{
    public interface IRedisCache
    {
        Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getItemCallback, TimeSpan? expiry = null);
        Task RemoveCacheAsync(string cacheKey);
        Task RemoveCacheStartWithAsync(string prefix);
        Task FlushAllDatabasesAsync();
        Task FlushDatabaseAsync(int db);
        Task SetStringAsync(string cacheKey, string value, TimeSpan? expiry = null);
        Task<T?> Get<T>(string cacheKey);
        Task<string?> GetStringAsync(string cacheKey);
    }
}
