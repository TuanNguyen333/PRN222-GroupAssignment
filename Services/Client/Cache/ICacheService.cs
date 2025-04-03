using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Client.Cache
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, int expirationMinutes = 5);
        Task DeleteAsync(string key);
        Task<bool> IsInWhitelist(int userId, string tokenFromRequest);
    }
}
