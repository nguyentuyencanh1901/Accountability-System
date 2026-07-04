using Example.Common.Const;
using Example.Common.Utilities.Helper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Example.Common.Cache
{
    public class RedisCache : IRedisCache
    {
        //private readonly IConnectionMultiplexer _connectionMultiplexer;

        //public RedisCache(IConnectionMultiplexer connectionMultiplexer)
        //{
        //    _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        //}

        //public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItemCallback, TimeSpan? expiry = null)
        //{
        //    var cache = _connectionMultiplexer.GetDatabase();

        //    var cachedData = await cache.StringGetAsync(key);
        //    if (cachedData.HasValue)
        //    {
        //        return JsonSerializer.Deserialize<T>(cachedData);
        //    }

        //    T? item = await getItemCallback();

        //    await cache.StringSetAsync(key, JsonSerializer.Serialize(item), expiry);

        //    return item;
        //}

        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly IServer _server;
        private readonly ILogger<RedisCache> _logger;

        public RedisCache(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCache> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _connectionMultiplexer = connectionMultiplexer;

            _database = _connectionMultiplexer.GetDatabase();
            var endpoints = _database.Multiplexer.GetEndPoints();
            _server = _connectionMultiplexer.GetServer(endpoints.First());
            _logger = logger;
        }

        /// <summary>Đọc cache Redis hoặc gọi DB; tắt khi RedisSettings:AllowCache = false.</summary>
        public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getItemCallback, TimeSpan? expiry = null)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    // Header example-refreshcache: refreshcache → bỏ qua cache cho request này
                    if (CacheHelper.Instance.IsRequestClearCache())
                    {
                        await RemoveCacheAsync(cacheKey);
                    }
                    else
                    {
                        var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                        if (!string.IsNullOrEmpty(cachedData))
                        {
                            var obj = JsonSerializer.Deserialize<T>(cachedData);
                            return obj;
                        }
                    }

                    // Cache miss → gọi DB rồi ghi Redis
                    var item = await getItemCallback();
                    if (item != null)
                    {
                        var options = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = expiry
                        };

                        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(item), options);
                    }
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);

                    return await getItemCallback();
                }
            }
            else
            {
                return await getItemCallback();
            }
        }

        public async Task RemoveCacheAsync(string cacheKey)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    await _distributedCache.RemoveAsync(cacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task RemoveCacheStartWithAsync(string prefix)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    var allKey = _server.Keys(pattern: $"{prefix}*");
                    foreach (var key in allKey)
                    {
                        await RemoveCacheAsync(key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }
        }

        public async Task FlushAllDatabasesAsync()
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    await _server.FlushAllDatabasesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }
        }

        public async Task FlushDatabaseAsync(int db)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    await _server.FlushDatabaseAsync(db);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task SetStringAsync(string cacheKey, string value, TimeSpan? expiry = null)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiry
                    };

                    await _distributedCache.SetStringAsync(cacheKey, value, options);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task<T?> Get<T>(string cacheKey)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                    if (!string.IsNullOrEmpty(cachedData))
                    {
                        var obj = JsonSerializer.Deserialize<T>(cachedData);
                        return obj;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            return default;
        }

        public async Task<string?> GetStringAsync(string cacheKey)
        {
            if (CacheConst.AllowCache)
            {
                try
                {
                    var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                    return cachedData;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
            return null;
        }
    }
}
