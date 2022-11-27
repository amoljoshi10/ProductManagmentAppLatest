using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using CachingService;

namespace CachingService
{
    public class CacheService : ICacheService
    {
        private readonly IInMemoryCacheService _memoryCache;
        private readonly IRedisCacheService _redisCache;
        private readonly IWebHostEnvironment _env;
        public CacheService(IWebHostEnvironment env,IInMemoryCacheService memoryCache,IRedisCacheService redisCache)
        {
            _env = env;
            _memoryCache = memoryCache;
            _redisCache = redisCache;
        }
        public string GetData(string cacheKey)
        {
            
            if (_env.EnvironmentName.Equals("Development"))
            {
                return _memoryCache.GetData(cacheKey);
            }
            else
            {
                return _redisCache.GetData(cacheKey);
            }
        }

        public void SetData(string cacheKey, string data)
        {
            if (_env.EnvironmentName.Equals("Development"))
            {
                _memoryCache.SetData(cacheKey,data);
            }
            else
            {
                _redisCache.SetData(cacheKey, data);
            }
        }
    }
}
