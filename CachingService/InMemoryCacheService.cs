using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CachingService
{
    public class InMemoryCacheService : IInMemoryCacheService
    {
        private readonly IMemoryCache _cache;
        public InMemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;    
        }
        public string GetData(string cacheKey)
        {
            return _cache.Get<string>(cacheKey);
        }

        public void SetData(string cacheKey, string data)
        {
            _cache.Set<string>(cacheKey,data);
        }
    }
}
