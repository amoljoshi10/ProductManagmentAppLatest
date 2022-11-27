
using CachingService;

namespace CachingService
{
    public interface ICacheService
    {
        string GetData(string cacheKey);
        void SetData(string cacheKey, string data);
    }
}
