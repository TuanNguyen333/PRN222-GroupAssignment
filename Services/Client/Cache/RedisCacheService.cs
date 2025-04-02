using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Client.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetAsync<T>(string key, T value, int expirationMinutes = 30)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
            };

            string jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            string? jsonData = await _cache.GetStringAsync(key);
            return jsonData != null ? JsonSerializer.Deserialize<T>(jsonData) : default;
        }

        public async Task DeleteAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<bool> IsInWhitelist(int userId, string tokenFromRequest)
        {
            string? tokenFromRedis = await GetAsync<string>($"whitelist:{userId}");
            return tokenFromRedis is not null && tokenFromRedis == tokenFromRequest;
        }
    }
}
