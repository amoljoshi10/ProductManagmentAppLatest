
namespace CachingService
{
    public interface IRedisCacheService
    {
        string GetData(string cacheKey);
        void SetData(string cacheKey, string data);
    }
}
